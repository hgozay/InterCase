(function () {
    'use strict';

    var POLL_INTERVAL_MS = 3000;
    var MAX_POLL_FAILURES = 5;
    var pollTimer = null;
    var pollFailures = 0;

    function newGuid() {
        var bytes = new Uint8Array(16);
        crypto.getRandomValues(bytes);
        bytes[6] = (bytes[6] & 0x0f) | 0x40;
        bytes[8] = (bytes[8] & 0x3f) | 0x80;
        var hex = Array.prototype.map.call(bytes, function (b) {
            return (b + 0x100).toString(16).slice(1);
        }).join('');
        return [hex.slice(0, 8), hex.slice(8, 12), hex.slice(12, 16), hex.slice(16, 20), hex.slice(20)].join('-');
    }

    document.addEventListener('click', function (event) {
        var guidButton = event.target.closest('[data-new-guid]');
        if (guidButton) {
            var input = document.querySelector(guidButton.getAttribute('data-new-guid'));
            if (input) {
                input.value = newGuid();
                input.focus();
            }
            return;
        }

        var demoButton = event.target.closest('[data-fill-demo]');
        if (demoButton) {
            var username = document.getElementById('Username');
            var password = document.getElementById('Password');
            if (username && password) {
                username.value = demoButton.getAttribute('data-username');
                password.value = demoButton.getAttribute('data-password');
                password.focus();
            }
        }
    });

    document.addEventListener('submit', function (event) {
        var form = event.target;
        var busyText = form.getAttribute('data-busy-text');
        var button = form.querySelector('button[type="submit"]');
        if (!busyText || !button) {
            return;
        }

        button.setAttribute('data-idle-text', button.textContent);
        button.textContent = busyText;
        button.disabled = true;
    });

    window.addEventListener('pageshow', function (event) {
        if (!event.persisted) {
            return;
        }

        document.querySelectorAll('button[data-idle-text]').forEach(function (button) {
            button.textContent = button.getAttribute('data-idle-text');
            button.disabled = false;
        });
    });

    function currentList() {
        return document.querySelector('[data-order-list]');
    }

    function schedulePoll(delay) {
        clearTimeout(pollTimer);
        var list = currentList();
        if (!list || list.getAttribute('data-active') !== 'true') {
            return;
        }

        pollTimer = setTimeout(poll, delay);
    }

    function poll() {
        if (document.hidden) {
            return;
        }

        var list = currentList();
        if (!list) {
            return;
        }

        fetch(list.getAttribute('data-url'), {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(function (response) {
                if (response.status === 401) {
                    window.location.reload();
                    return null;
                }
                if (!response.ok) {
                    throw new Error('HTTP ' + response.status);
                }
                return response.text();
            })
            .then(function (html) {
                if (html === null) {
                    return;
                }

                var current = currentList();
                if (current) {
                    current.outerHTML = html;
                }
                pollFailures = 0;
                schedulePoll(POLL_INTERVAL_MS);
            })
            .catch(function () {
                pollFailures += 1;
                if (pollFailures < MAX_POLL_FAILURES) {
                    schedulePoll(POLL_INTERVAL_MS * (pollFailures + 1));
                }
            });
    }

    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) {
            schedulePoll(0);
        }
    });

    schedulePoll(POLL_INTERVAL_MS);
})();
