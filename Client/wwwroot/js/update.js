const bc = new BroadcastChannel('blazor-channel');

bc.onmessage = function (message) {
    if (message) {
        switch (message.data) {
            case 'update-installing':
                console.info('update installing (0%)');
                break;
            case 'update-installed':
                console.info('update installed (100%)');
                bc.postMessage('update-restart');
                break;
            case 'update-completed':
                console.info('update completed');
                window.location.href = window.location.href;
                break;
        }
    }
}
