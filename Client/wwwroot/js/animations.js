(function () {
    window.blazorAnimations = {
        addRippleToButton: (element) => {
            element.addEventListener('click', createRipple);
        },
        focus: (elementId) => {
            const element = document.getElementById(elementId);
            element.scrollIntoView();
            element.focus();
        }
    };

    function createRipple(e) {
        const button = e.currentTarget;
        const rippleSpan = button.getElementsByClassName('ripple')[0];
        const diameter = Math.max(button.clientWidth, button.clientHeight);
        const radius = diameter / 2;
        const pos = button.getBoundingClientRect();

        rippleSpan.style.width = rippleSpan.style.height = `${diameter}px`;
        rippleSpan.style.left = `${e.clientX - pos.left - radius}px`;
        rippleSpan.style.top = `${e.clientY - pos.top - radius}px`;

        rippleSpan.remove();
        button.appendChild(rippleSpan);
    }
})();
