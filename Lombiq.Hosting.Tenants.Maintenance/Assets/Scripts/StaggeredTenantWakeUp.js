/* global bootstrap */

let pollingInterval = null;
let countdownTimer = null;
let pollingRate = 10000;
let countdown = pollingRate / 1000;
const maintenanceStatusContainer = document.getElementById('maintenance-status-container');
const url = maintenanceStatusContainer.getAttribute('data-maintenance-url');

function updateCountdownDisplay(clear = false) {
    const el = document.getElementById('refreshCountdown');
    if (!el) return;

    el.textContent = clear ? '' : `Next refresh in ${countdown}s`;
}

function stopPolling() {
    if (pollingInterval !== null) {
        clearInterval(pollingInterval);
        pollingInterval = null;
    }

    if (countdownTimer !== null) {
        clearInterval(countdownTimer);
        countdownTimer = null;
    }

    updateCountdownDisplay(true);
}

// Animation.
function animateNumber(element, start, target) {
    const duration = 800;
    const startTime = performance.now();

    function step(currentTime) {
        const progress = Math.min((currentTime - startTime) / duration, 1);
        const value = Math.floor(progress * (target - start) + start);
        element.textContent = value + ' %';
        if (progress < 1) requestAnimationFrame(step);
    }

    requestAnimationFrame(step);
}

function updateMaintenanceContent(html) {
    const container = document.getElementById('maintenance-status-container');
    const oldBar = document.getElementById('maintenance-progress-bar');
    const oldWidth = oldBar?.style.width || '0%';
    const oldNumber = oldBar ? parseInt(oldBar.textContent, 10) : 0;

    container.innerHTML = html;

    const newBar = document.getElementById('maintenance-progress-bar');
    if (newBar) {
        const targetWidth = newBar.getAttribute('aria-valuenow') + '%';
        const newPercentage = parseInt(newBar.getAttribute('aria-valuenow') || '0', 10);

        // Animate width from old to new value.
        newBar.animate(
            [
                { width: oldWidth },
                { width: targetWidth },
            ],
            {
                duration: 800,
                easing: 'ease-in-out',
                fill: 'forwards',
            }
        );

        animateNumber(newBar, oldNumber, newPercentage);
    }

    const isRunning = container.querySelector('[data-maintenance-running]')?.dataset.maintenanceRunning === 'true';
    if (!isRunning) stopPolling();

    // Re-initialize popovers.
    document.querySelectorAll('[data-bs-toggle="popover"]').forEach((el) => {
        if (!bootstrap.Popover.getInstance(el)) {
            new bootstrap.Popover(el);
        }
    });
}

function pollNow() {
    if (document.hidden) return;

    fetch(url)
        .then((response) => response.text())
        .then(updateMaintenanceContent);
}

// Countdown.
function startCountdown() {
    countdown = pollingRate / 1000;
    updateCountdownDisplay();

    countdownTimer = setInterval(() => {
        countdown -= 1;
        updateCountdownDisplay();
    }, 1000);
}

// Core polling.
function startPolling() {
    stopPolling();

    pollNow();

    pollingInterval = setInterval(() => {
        pollNow();
        countdown = pollingRate / 1000;
    }, pollingRate);

    startCountdown();
}

// Initialization.
const toggle = document.getElementById('autoRefreshToggle');
const rateSelect = document.getElementById('refreshRateSelect');

const savedToggle = localStorage.getItem('autoRefreshEnabled');
const savedRate = localStorage.getItem('autoRefreshRate');

if (savedToggle !== null) toggle.checked = savedToggle === 'true';
if (savedRate !== null) {
    rateSelect.value = savedRate;
    pollingRate = parseInt(savedRate, 10);
}

if (toggle.checked) startPolling();

toggle.addEventListener('change', () => {
    localStorage.setItem('autoRefreshEnabled', toggle.checked);
    if (toggle.checked) {
        startPolling();
    }
    else {
        stopPolling();
    }
});

rateSelect.addEventListener('change', () => {
    pollingRate = parseInt(rateSelect.value, 10);
    localStorage.setItem('autoRefreshRate', pollingRate);
    if (toggle.checked) startPolling();
});

// Tab visibility handler.
document.addEventListener('visibilitychange', () => {
    if (!document.hidden && toggle.checked) {
        startPolling();
    }
});

document.querySelectorAll('[data-bs-toggle="popover"]').forEach((el) => {
    if (!bootstrap.Popover.getInstance(el)) {
        new bootstrap.Popover(el);
    }
});
