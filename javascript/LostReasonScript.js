function onFormLoad(executionContext) {
    // Guard clause — exit if no context
    if (!executionContext) return;

    var formContext = executionContext.getFormContext();
    if (!formContext) return;

    var statusField = formContext
        .getAttribute("propbot_pipelinestage");

    if (statusField) {
        // Add onChange handler
        statusField.addOnChange(onStatusChange);
        // Run once on load
        onStatusChange(executionContext);
    }
}

function onStatusChange(executionContext) {
    if (!executionContext) return;

    var formContext = executionContext.getFormContext();
    if (!formContext) return;

    var statusAttr = formContext
        .getAttribute("propbot_pipelinestage");
    var lostReasonAttr = formContext
        .getAttribute("propbot_lostreason");
    var lostReasonControl = formContext
        .getControl("propbot_lostreason");

    if (!statusAttr || !lostReasonAttr || 
        !lostReasonControl) return;

    var status = statusAttr.getValue();
    var isClosedLost = (status === 782280004);

    // Clear any existing notification first
    formContext.ui.clearFormNotification("suggestionId");

    if (isClosedLost) {
        // Show and require Lost Reason
        lostReasonControl.setVisible(true);
        lostReasonAttr.setRequiredLevel("required");

        // Show smart suggestion as INFO banner at top
        var suggestion = getSuggestion(formContext);
        formContext.ui.setFormNotification(
            "💡 Suggested reason: " + suggestion,
            "INFO",
            "suggestionId");
    } else {
        // Hide and unrequire Lost Reason
        lostReasonControl.setVisible(false);
        lostReasonAttr.setRequiredLevel("none");
        lostReasonControl.clearNotification("suggestionId");
    }
}

function getSuggestion(formContext) {
    var viewingsCount = getViewingsCount(formContext);
    var daysInStage = getDaysInStage(formContext);

    if (viewingsCount > 3) {
        return "Buyer Indecisive";
    } else if (daysInStage > 14) {
        return "Price Disagreement";
    } else if (viewingsCount <= 1) {
        return "Property Not Suitable";
    } else {
        return "Buyer Bought Elsewhere";
    }
}

function getViewingsCount(formContext) {
    // Returns 0 for now
    // Can be enhanced later to query Viewings table
    return 0;
}

function getDaysInStage(formContext) {
    // Calculate days since record was modified
    var modifiedOn = formContext
        .getAttribute("modifiedon");
    if (!modifiedOn) return 0;

    var modifiedDate = modifiedOn.getValue();
    if (!modifiedDate) return 0;

    var today = new Date();
    var diff = today - modifiedDate;
    return Math.floor(diff / (1000 * 60 * 60 * 24));
}