document.addEventListener('DOMContentLoaded', function() {
    const loginSection = document.getElementById('login-section');
    const chatSection = document.getElementById('chat-section');
    const loginForm = document.getElementById('login-form');
    const chatForm = document.getElementById('chat-form');
    const chatInput = document.getElementById('chat-input');
    const chatOutput = document.getElementById('chat-output');
    const logoutBtn = document.getElementById('logout-btn');
    const loginError = document.getElementById('login-error');

    // Simple login: just show chat section
    loginForm.addEventListener('submit', function(e) {
        e.preventDefault();
        loginSection.style.display = 'none';
        chatSection.style.display = 'flex';
        chatInput.focus();
    });

    logoutBtn.addEventListener('click', function() {
        chatSection.style.display = 'none';
        loginSection.style.display = 'flex';
        chatOutput.textContent = '';
        chatInput.value = '';
    });

    chatForm.addEventListener('submit', function(e) {
        e.preventDefault();
        const text = chatInput.value.trim();
        if (!text) return;
        chatInput.value = '';
        chatOutput.textContent = '';
        // Stream response from backend
        const evtSource = new EventSource(`/stream-sse?text=${encodeURIComponent(text)}`);
        evtSource.onmessage = function(event) {
            chatOutput.textContent += event.data;
        };
        evtSource.onerror = function() {
            evtSource.close();
        };
    });
}); 