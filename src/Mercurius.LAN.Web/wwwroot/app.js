// This function will be called from your Blazor component.
function addOutsideClickListener(elementId, dotNetHelper) {
    // Get a reference to the element.
    const element = document.getElementById(elementId);

    // This is the listener function that will be added to the document.
    const listener = (event) => {
        // Check if the clicked element is outside of our component.
        // We check if the element exists and if the click target is not inside it.
        if (element && !element.contains(event.target)) {
            // Invoke the C# method to close the dropdown.
            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }
    };

    // Add the listener to the document.
    document.addEventListener('click', listener);

    // Create a cleanup function to remove the listener later.
    return {
        dispose: () => {
            document.removeEventListener('click', listener);
        }
    };
}