# PropBot AI — Real Estate CRM on Microsoft Dynamics 365

An AI-powered real estate CRM built on Microsoft Dynamics 365 Sales and Microsoft Power Platform. Built as part of a Digital Transformation course project.

---

## The Problem

Real estate agencies in Karachi manage leads on WhatsApp and track deals in Excel. Agents miss follow ups, managers have no visibility, and commission calculations are done manually with frequent errors.

---

## The Solution

PropBot AI is a complete enterprise CRM that manages the full property deal lifecycle — from first buyer inquiry to closed deal and commission payout — with an AI chatbot called PropBot that agents use directly in Microsoft Teams.

---

## Tech Stack

| Layer | Technology |
|---|---|
| CRM Pipeline | Microsoft Dynamics 365 Sales |
| Database | Microsoft Dataverse |
| Automation | Power Automate |
| Server Logic | C# Plugins |
| Client Logic | JavaScript Form Scripts |
| AI Chatbot | Microsoft Copilot Studio |
| Reporting | Power BI |
| Deployment | Microsoft Teams |

---

## Key Features

### C# Plugin 1 — Duplicate Check and Lead Score
- Trigger: Post-operation, Create, propbot_inquiry
- Checks if same buyer already has active inquiry for same property
- Blocks duplicate and shows error message
- Sets initial Lead Score to 50 on all new inquiries

### C# Plugin 2 — Commission and Property Lifecycle
- Trigger: Post-operation, Update, Closed Won stage
- Calculates 3% commission for Sale or 5% for Rent
- Creates Commission record in Dataverse automatically
- Updates Property status to Sold or Rented
- Calculates Days on Market from listing date to close date

### Power Automate Flows
- Flow 1 — Round robin agent assignment on new inquiry
- Flow 2 — Stale lead alert after 3 days of no activity
- Flow 3 — Full deal closure sequence with Teams notification, buyer email, admin task, and audit log
- Flow 4 — High value deal approval for deals above 1 crore

### PropBot — Copilot Studio Chatbot in Teams
Four conversation intents connected to live Dataverse data:
1. Property Search — find available properties by area and budget
2. Inquiry Status — check stage and lead score for any buyer
3. Create Inquiry — log new lead directly from Teams
4. My Performance — personal stats including deals closed and commission earned

### Security Roles
Three custom roles configured in Dataverse:
- PropBot Agent — sees only own records
- PropBot Manager — full access across all agents
- PropBot Admin — manages property master data only

---

## Pipeline Stages

Inquiry → Viewing Scheduled → Offer Made → Closed Won → Closed Lost

---

## Database Tables

- Property — listings with area, type, price, status, days on market
- Inquiry — buyer leads with pipeline stage, lead score, offer amount
- Commission — calculated commission per closed deal
- Viewing — scheduled site visits with outcome and feedback
- AuditLog — system events including deal closures and duplicate attempts

---

## Screenshots

See the screenshots folder for:
- PropBot CRM Model Driven App
- PropBot chatbot in Microsoft Teams
- Plugin duplicate block error
- Power BI management dashboard
- Commission record auto-created by plugin

---

## Project Details

Course: Digital Transformation
Built by: Anaksha Janki, Rania Ghazanfar, Marium Arif
Duration: May 2026 — June 2026
Environment: Microsoft Dynamics 365 Sales Trial
