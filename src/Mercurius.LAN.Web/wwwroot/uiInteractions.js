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

// Copy-to-clipboard helper. Use `copyToClipboardFromElement(this)` from markup.
function copyToClipboard(text) {
    if (!text && text !== "") return;
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).catch(function () { /* ignore */ });
        return;
    }
    // Fallback for older browsers
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.setAttribute('readonly', '');
    textarea.style.position = 'absolute';
    textarea.style.left = '-9999px';
    document.body.appendChild(textarea);
    textarea.select();
    try { document.execCommand('copy'); } catch (e) { /* ignore */ }
    document.body.removeChild(textarea);
}

function copyToClipboardFromElement(el) {
    if (!el) return;
    const value = el.getAttribute('data-copy');
    if (value !== null) {
        copyToClipboard(value);
        // small visual feedback: briefly add a class
        el.classList.add('copied');
        setTimeout(() => el.classList.remove('copied'), 900);
        // show a small toast message
        showCopyToast('Copied: ' + value, el);
        return;
    }
    copyToClipboard(el.textContent || '');
}

// Small JS toast for copy feedback. Appears bottom-right.
function showCopyToast(message, /* optional */ anchorEl) {
    try {
        const toast = document.createElement('div');
        toast.className = 'js-copy-toast';
        toast.textContent = message;

        document.body.appendChild(toast);

        // force reflow to allow transition
        // position near anchor if possible
        if (anchorEl && anchorEl.getBoundingClientRect) {
            const rect = anchorEl.getBoundingClientRect();
            // if there's enough space, show above the anchor; otherwise bottom-right
            const top = rect.top - 8 - toast.offsetHeight;
            if (top > 8) {
                toast.style.position = 'absolute';
                toast.style.left = Math.max(8, rect.left) + 'px';
                toast.style.top = (window.scrollY + rect.top - 40) + 'px';
            } else {
                toast.style.position = 'fixed';
                toast.style.right = '20px';
                toast.style.bottom = '20px';
            }
        } else {
            toast.style.position = 'fixed';
            toast.style.right = '20px';
            toast.style.bottom = '20px';
        }

        // show
        requestAnimationFrame(() => toast.classList.add('show'));

        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => { try { document.body.removeChild(toast); } catch (e) {} }, 220);
        }, 900);
    } catch (e) {
        // ignore
    }
}