const bc = new BroadcastChannel('blazor-channel');

bc.onmessage = function (message) {
    if (message) {
        switch (message.data) {
            case 'update-installing':
                console.info('update installing (0%)');

                const updateDialog = document.getElementById('blazor-update');
                updateDialog.style.display = 'inherit';
                break;
            case 'update-installed':
                console.info('update installed (100%)');

                const updateBtn = document.getElementById('blazor-update-btn');
                updateBtn.innerText = 'Update now';
                updateBtn.disabled = false;
                updateBtn.addEventListener('click', () => {
                    bc.postMessage('update-restart');
                    updateBtn.disabled = true;
                    updateBtn.innerText = 'Applying update. The app will restart shortly.';
                });
                break;
            case 'update-completed':
                console.info('update completed');

                window.location.href = window.location.href;
                break;
        }
    }
}
