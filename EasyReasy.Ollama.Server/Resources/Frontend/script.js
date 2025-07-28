document.addEventListener('DOMContentLoaded', function() {
    const loginSection = document.getElementById('login-section');
    const chatSection = document.getElementById('chat-section');
    const loginForm = document.getElementById('login-form');
    const chatForm = document.getElementById('chat-form');
    const chatInput = document.getElementById('chat-input');
    const chatOutput = document.getElementById('chat-output');
    const logoutBtn = document.getElementById('logout-btn');
    const loginError = document.getElementById('login-error');

    let authToken = null;

    // Check if we have a stored token
    const storedToken = localStorage.getItem('authToken');
    if (storedToken) {
        authToken = storedToken;
        loginSection.style.display = 'none';
        chatSection.style.display = 'flex';
        chatInput.focus();
    }

    // Handle login form submission
    loginForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const apiKey = document.getElementById('api-key').value.trim();
        
        if (!apiKey) {
            showLoginError('Please enter an API key');
            return;
        }

        try {
            const response = await fetch('/api/auth/apikey', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ apiKey: apiKey })
            });

            if (response.ok) {
                const data = await response.json();
                authToken = data.token;
                localStorage.setItem('authToken', authToken);
                
                loginSection.style.display = 'none';
                chatSection.style.display = 'flex';
                chatInput.focus();
                hideLoginError();
            } else {
                showLoginError('Invalid API key');
            }
        } catch (error) {
            console.error('Login error:', error);
            showLoginError('Login failed. Please try again.');
        }
    });

    // Handle logout
    logoutBtn.addEventListener('click', function() {
        authToken = null;
        localStorage.removeItem('authToken');
        chatSection.style.display = 'none';
        loginSection.style.display = 'flex';
        chatOutput.textContent = '';
        chatInput.value = '';
        document.getElementById('api-key').value = '';
        hideLoginError();
    });

    // Handle chat form submission
    chatForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const text = chatInput.value.trim();
        if (!text) return;
        
        if (!authToken) {
            showLoginError('Please login first');
            return;
        }
        
        // Clear input and output
        chatInput.value = '';
        chatOutput.textContent = '';
        
        try {
            // Use fetch with streaming instead of EventSource for authentication support
            const response = await fetch(`/stream-sse?text=${encodeURIComponent(text)}`, {
                headers: {
                    'Authorization': `Bearer ${authToken}`,
                    'Accept': 'text/event-stream'
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    showLoginError('Authentication failed. Please login again.');
                    logoutBtn.click();
                } else {
                    showLoginError('Request failed. Please try again.');
                }
                return;
            }

            const reader = response.body.getReader();
            const decoder = new TextDecoder();

            while (true) {
                const { done, value } = await reader.read();
                if (done) break;

                const chunk = decoder.decode(value);
                const lines = chunk.split('\n');

                for (const line of lines) {
                    if (line.startsWith('data: ')) {
                        const data = line.substring(6); // Remove 'data: ' prefix
                        if (data.trim()) {
                            try {
                                // Parse the ChatResponsePart JSON object
                                const responsePart = JSON.parse(data);
                                
                                // Handle message content
                                if (responsePart.message) {
                                    chatOutput.textContent += responsePart.message;
                                }
                                
                                // Handle tool calls
                                if (responsePart.toolCall) {
                                    const toolCall = responsePart.toolCall;
                                    const toolCallDiv = document.createElement('div');
                                    toolCallDiv.className = 'tool-call';
                                    toolCallDiv.innerHTML = `
                                        <div class="tool-call-header">
                                            <strong>Tool Call:</strong> ${toolCall.name || 'Unknown Tool'}
                                        </div>
                                        <div class="tool-call-content">
                                            <pre>${JSON.stringify(toolCall, null, 2)}</pre>
                                        </div>
                                    `;
                                    chatOutput.appendChild(toolCallDiv);
                                }
                            } catch (error) {
                                console.error('Error parsing response part:', error);
                                // Fallback: just append the raw data
                                chatOutput.textContent += data;
                            }
                        }
                    }
                }
            }
        } catch (error) {
            console.error('Streaming error:', error);
            showLoginError('Connection failed. Please try again.');
        }
    });

    function showLoginError(message) {
        loginError.textContent = message;
        loginError.style.display = 'block';
    }

    function hideLoginError() {
        loginError.style.display = 'none';
        loginError.textContent = '';
    }
}); 