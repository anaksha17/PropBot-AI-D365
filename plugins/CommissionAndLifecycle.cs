using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PropBotPlugins
{
    public class CommissionAndLifecycle : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            ITracingService tracing =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity target =
                    (Entity)context.InputParameters["Target"];

                if (!target.Contains("propbot_pipelinestage"))
                {
                    tracing.Trace("No stage change. Exiting.");
                    return;
                }

                OptionSetValue stageValue =
                    target.GetAttributeValue<OptionSetValue>(
                        "propbot_pipelinestage");

                tracing.Trace("Stage value received: " +
                    (stageValue != null ?
                        stageValue.Value.ToString() : "NULL"));

                // 782280003 = Closed Won
                if (stageValue == null ||
                    stageValue.Value != 782280003)
                {
                    tracing.Trace("Not Closed Won. Exiting.");
                    return;
                }

                tracing.Trace("Closed Won detected. Running...");

                Guid inquiryId = target.Id;

                // Retrieve full inquiry — agent comes from here only
                Entity fullInquiry = service.Retrieve(
                    "propbot_inquiry",
                    inquiryId,
                    new ColumnSet(
                        "propbot_agreedprice",
                        "propbot_dealtype",
                        "propbot_linkedproperty",
                        "propbot_agent",
                        "propbot_buyername"));

                Money agreedPrice =
                    fullInquiry.GetAttributeValue<Money>(
                        "propbot_agreedprice");

                OptionSetValue dealType =
                    fullInquiry.GetAttributeValue<OptionSetValue>(
                        "propbot_dealtype");

                EntityReference linkedProperty =
                    fullInquiry.GetAttributeValue<EntityReference>(
                        "propbot_linkedproperty");

                // Agent lookup on Inquiry → points to Agent2 table
                EntityReference linkedAgent =
                    fullInquiry.GetAttributeValue<EntityReference>(
                        "propbot_agent");

                tracing.Trace("Agreed Price: " +
                    (agreedPrice != null ?
                        agreedPrice.Value.ToString() : "NULL"));

                tracing.Trace("Deal Type: " +
                    (dealType != null ?
                        dealType.Value.ToString() : "NULL"));

                tracing.Trace("Linked Agent Id: " +
                    (linkedAgent != null ?
                        linkedAgent.Id.ToString() : "NULL"));

                if (agreedPrice == null)
                {
                    tracing.Trace("No agreed price. Exiting.");
                    return;
                }

                decimal commissionRate = 0.03m;
                string dealTypeLabel = "Sale";

                if (dealType != null && dealType.Value == 782280001)
                {
                    commissionRate = 0.05m;
                    dealTypeLabel = "Rent";
                }

                decimal commissionAmount =
                    agreedPrice.Value * commissionRate;

                tracing.Trace("Commission Rate: " +
                    commissionRate.ToString());
                tracing.Trace("Commission Amount: " +
                    commissionAmount.ToString());

                // Get Agent Name from Agent2 table (for title only)
                string agentName = "Unknown Agent";

                if (linkedAgent != null)
                {
                    Entity agentRecord = service.Retrieve(
                        "propbot_agent",
                        linkedAgent.Id,
                        new ColumnSet("propbot_name"));

                    string fetchedName =
                        agentRecord.GetAttributeValue<string>(
                            "propbot_name");

                    if (!string.IsNullOrEmpty(fetchedName))
                    {
                        agentName = fetchedName;
                    }
                }

                tracing.Trace("Agent Name: " + agentName);

                // ========================================
                // CREATE COMMISSION RECORD
                // ========================================
                Entity commission =
                    new Entity("propbot_dealcommission");

                // ONLY link to Inquiry — no direct agent field
                commission["propbot_linkedinquiry"] =
                    new EntityReference(
                        "propbot_inquiry", inquiryId);

                commission["propbot_agreedprice"] =
                    new Money(agreedPrice.Value);

                commission["propbot_commissionrate"] =
                    commissionRate * 100;

                commission["propbot_commissionamount"] =
                    new Money(commissionAmount);

                commission["propbot_paymentstatus"] =
                    new OptionSetValue(782280000);

                // Primary column — show Agent Name in title
                commission["propbot_dealcommission1"] =
                    "Commission for " + agentName;

                service.Create(commission);
                tracing.Trace(
                    "Commission record created for agent: " +
                    agentName);

                // ========================================
                // UPDATE PROPERTY STATUS
                // ========================================
                if (linkedProperty != null)
                {
                    Entity propertyUpdate =
                        new Entity("propbot_property");

                    propertyUpdate.Id = linkedProperty.Id;

                    int newStatus = 782280002; // Sold
                    if (dealTypeLabel == "Rent")
                    {
                        newStatus = 782280003; // Rented
                    }

                    propertyUpdate["propbot_status"] =
                        new OptionSetValue(newStatus);

                    Entity fullProperty = service.Retrieve(
                        "propbot_property",
                        linkedProperty.Id,
                        new ColumnSet("propbot_listingdate"));

                    DateTime? listingDate =
                        fullProperty.GetAttributeValue<DateTime?>(
                            "propbot_listingdate");

                    if (listingDate.HasValue)
                    {
                        int daysOnMarket =
                            (DateTime.UtcNow -
                            listingDate.Value).Days;

                        propertyUpdate["propbot_daysonmarket"] =
                            daysOnMarket;

                        tracing.Trace("Days on Market: " +
                            daysOnMarket.ToString());
                    }

                    service.Update(propertyUpdate);
                    tracing.Trace("Property updated to " +
                        dealTypeLabel + ".");
                }

                tracing.Trace(
                    "Plugin 2 completed successfully.");
            }
        }
    }
}