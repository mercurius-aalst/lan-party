// JavaScript function to make an element horizontally draggable
function makeDraggable(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;

    let isDragging = false;
    let startX;
    let scrollLeft;

    element.addEventListener('mousedown', (e) => {
        isDragging = true;
        startX = e.pageX - element.offsetLeft;
        scrollLeft = element.scrollLeft;
    });

    element.addEventListener('mouseleave', () => {
        isDragging = false;
    });

    element.addEventListener('mouseup', () => {
        isDragging = false;
    });

    element.addEventListener('mousemove', (e) => {
        if (!isDragging) return;
        e.preventDefault();
        const x = e.pageX - element.offsetLeft;
        const walk = (x - startX) * 2; // Adjust scrolling speed
        element.scrollLeft = scrollLeft - walk;
    });
}

// JavaScript function to toggle the sidebar visibility on mobile
function showSidebarToggleOnMobile() {
    // Add your implementation here if needed
}