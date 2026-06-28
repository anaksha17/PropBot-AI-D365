
# Power Automate Flows

## Flow 1 — Round Robin Assignment
Trigger: When new Inquiry record is created in Dataverse.
Logic: Counts active inquiries per agent. Assigns to least busy agent. Sends Teams notification with inquiry details.

## Flow 2 — Stale Lead Alert
Trigger: Scheduled daily at 8am.
Logic: Queries all open inquiries where Last Modified On is more than 3 days ago. Sends Teams alert to agent and manager. Reduces Lead Score by 10.

## Flow 3 — Deal Closure Sequence
Trigger: When Inquiry Pipeline Stage changes to Closed Won.
Logic: Sends congratulations Teams message to agent. Sends email to buyer. Creates admin paperwork task. Writes AuditLog record with full deal details.

## Flow 4 — High Value Deal Approval
Trigger: When Offer Amount on Inquiry exceeds 1 crore.
Logic: Sends approval request to manager in Teams with Approve and Reject buttons. Notifies agent of outcome. Manager approves directly from Teams without opening D365.

## Bot Flows
- BotFlow_PropertySearch — queries Property table and returns matching listings
- BotFlow_CheckInquiryStatus — queries Inquiry table by buyer name
- BotFlow_CreateInquiry — searches property by title and creates Inquiry record
- BotFlow_AgentPerformance — returns closed deals and commission for named agent
