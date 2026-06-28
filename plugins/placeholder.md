
This folder contains the C# plugin source code files written for Microsoft Dynamics 365.

Plugin 1 - DuplicateCheckAndLeadScore.cs
Fires on Post-operation Create of Inquiry. Checks for duplicate buyer and property combination. Blocks save if duplicate found. Sets initial Lead Score to 50.

Plugin 2 - CommissionAndLifecycle.cs
Fires on Post-operation Update of Inquiry when stage changes to Closed Won. Calculates commission at 3% for Sale or 5% for Rent. Creates Commission record. Updates Property status to Sold or Rented. Calculates Days on Market.
