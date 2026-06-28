using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PropBotPlugins
{
    public class DuplicateCheckAndLeadScore : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Tracing Service
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            // Context
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            // Organization Service
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity targetInquiry =
                    (Entity)context.InputParameters["Target"];

                // Set Lead Score = 50
                targetInquiry["propbot_leadscore"] = 50;

                // Get Buyer Name
                string buyerName = targetInquiry
                    .GetAttributeValue<string>("propbot_buyername");

                // Get Linked Property
                EntityReference linkedProperty = targetInquiry
                    .GetAttributeValue<EntityReference>(
                        "propbot_linkedproperty");

                // Trace what we received
                tracingService.Trace(
                    "BuyerName received: [" + buyerName + "]");
                tracingService.Trace(
                    "PropertyId received: [" +
                    linkedProperty?.Id.ToString() + "]");

                if (!string.IsNullOrEmpty(buyerName) &&
                    linkedProperty != null)
                {
                    // Check Property Status
                    Entity property = service.Retrieve(
                        "propbot_property",
                        linkedProperty.Id,
                        new ColumnSet("propbot_status"));

                    OptionSetValue propertyStatus =
                        property.GetAttributeValue<OptionSetValue>(
                            "propbot_status");

                    tracingService.Trace(
                        "Property Status Value: [" +
                        propertyStatus?.Value.ToString() + "]");

                    if (propertyStatus != null &&
                        (propertyStatus.Value == 782280002 ||
                         propertyStatus.Value == 782280003))
                    {
                        string statusText =
                            propertyStatus.Value == 782280002 ?
                            "Sold" : "Rented";

                        tracingService.Trace(
                            "Blocking — property is: " + statusText);

                        throw new InvalidPluginExecutionException(
                            "This property is already " + statusText +
                            ". You cannot create new inquiries " +
                            "for this property.");
                    }

                    // Duplicate Check
                    tracingService.Trace(
                        "Running duplicate check...");

                    QueryExpression query =
                        new QueryExpression("propbot_inquiry");
                    query.ColumnSet = new ColumnSet(
                        "propbot_inquiryid",
                        "propbot_buyername",
                        "propbot_pipelinestage");

                    // Same Buyer Name
                    query.Criteria.AddCondition(
                        "propbot_buyername",
                        ConditionOperator.Equal,
                        buyerName);

                    // Same Property
                    query.Criteria.AddCondition(
                        "propbot_linkedproperty",
                        ConditionOperator.Equal,
                        linkedProperty.Id);

                    // Active Stages Only
                    query.Criteria.AddCondition(
                        "propbot_pipelinestage",
                        ConditionOperator.In,
                        new object[] {
                            782280000,
                            782280001,
                            782280002 });

                    EntityCollection results =
                        service.RetrieveMultiple(query);

                    tracingService.Trace(
                        "Duplicate records found: " +
                        results.Entities.Count);

                    // Log each found record
                    foreach (Entity e in results.Entities)
                    {
                        string foundBuyer =
                            e.GetAttributeValue<string>(
                                "propbot_buyername");
                        OptionSetValue foundStage =
                            e.GetAttributeValue<OptionSetValue>(
                                "propbot_pipelinestage");
                        tracingService.Trace(
                            "Found — Buyer: [" + foundBuyer +
                            "] Stage: [" +
                            foundStage?.Value.ToString() + "]");
                    }

                    if (results.Entities.Count > 0)
                    {
                        throw new InvalidPluginExecutionException(
                            "Duplicate: This buyer already has " +
                            "an active inquiry for this property.");
                    }

                    tracingService.Trace(
                        "No duplicate found — allowing create.");
                }
            }
        }
    }
}