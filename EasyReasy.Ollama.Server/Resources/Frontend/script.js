document.addEventListener('DOMContentLoaded', function() {
    // Configure marked.js for security and rendering
    marked.setOptions({
        breaks: true, // Convert \n to <br>
        gfm: true, // GitHub Flavored Markdown
        headerIds: false, // Disable header IDs for security
        mangle: false, // Disable mangling for security
        sanitize: false, // We'll handle sanitization manually
        smartLists: true, // Use smarter list behavior
        smartypants: true, // Use smart typographic punctuation
        xhtml: false // Don't use XHTML output
    });

    const loginSection = document.getElementById('login-section');
    const chatSection = document.getElementById('chat-section');
    const loginForm = document.getElementById('login-form');
    const chatForm = document.getElementById('chat-form');
    const chatInput = document.getElementById('chat-input');
    const chatOutput = document.getElementById('chat-output');
    const logoutBtn = document.getElementById('logout-btn');
    const loginError = document.getElementById('login-error');
    const modelSelect = document.getElementById('model-select');

    let authToken = null;
    let conversationHistory = [];
    let availableModels = [];
    let selectedModel = '';

    // Check if we have a stored token
    const storedToken = localStorage.getItem('authToken');
    if (storedToken) {
        authToken = storedToken;
        loginSection.style.display = 'none';
        chatSection.style.display = 'flex';
        loadAvailableModels();
        chatInput.focus();
    }

    // Function to load available models
    async function loadAvailableModels() {
        if (!authToken) return;

        try {
            const response = await fetch('/api/ollama/models', {
                headers: {
                    'Authorization': `Bearer ${authToken}`
                }
            });

            if (response.ok) {
                availableModels = await response.json();
                populateModelSelect();
            } else {
                console.error('Failed to load models');
                populateModelSelect(); // Still populate with default
            }
        } catch (error) {
            console.error('Error loading models:', error);
            populateModelSelect(); // Still populate with default
        }
    }

    // Function to populate model select dropdown
    function populateModelSelect() {
        modelSelect.innerHTML = '';
        
        if (availableModels.length === 0) {
            // Add default model if no models are available
            const defaultOption = document.createElement('option');
            defaultOption.value = 'llama3.1';
            defaultOption.textContent = 'llama3.1 (default)';
            modelSelect.appendChild(defaultOption);
            selectedModel = 'llama3.1';
        } else {
            // Add all available models
            availableModels.forEach(model => {
                const option = document.createElement('option');
                option.value = model;
                option.textContent = model;
                modelSelect.appendChild(option);
            });
            
            // Set the first model as default
            if (availableModels.length > 0) {
                selectedModel = availableModels[0];
                modelSelect.value = selectedModel;
            }
        }
    }

    // Handle model selection change
    modelSelect.addEventListener('change', function() {
        selectedModel = this.value;
    });

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
                loadAvailableModels();
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
        conversationHistory = [];
        availableModels = [];
        selectedModel = '';
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

        if (!selectedModel) {
            showLoginError('Please select a model');
            return;
        }
        
        // Add user message to conversation history
        const userMessage = {
            role: 'user',
            content: text
        };
        conversationHistory.push(userMessage);
        
        // Clear input only, keep existing messages
        chatInput.value = '';
        
        // Display user message
        const userMessageDiv = document.createElement('div');
        userMessageDiv.className = 'message user-message';
        userMessageDiv.innerHTML = `
            <div class="message-header">
                <strong>You:</strong>
            </div>
            <div class="message-content">${text}</div>
        `;
        chatOutput.appendChild(userMessageDiv);
        
        try {
            // Prepare the chat request
            const chatRequest = {
                modelName: selectedModel,
                messages: conversationHistory,
                toolDefinitions: null // No tools for now, could be made configurable
            };

            // Use the proper ChatController endpoint
            const response = await fetch('/api/chat/stream', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${authToken}`,
                    'Content-Type': 'application/json',
                    'Accept': 'text/event-stream'
                },
                body: JSON.stringify(chatRequest)
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
            let assistantMessage = '';
            let assistantMessageDiv = null;

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
                                    assistantMessage += responsePart.message;
                                    
                                    // Create assistant message div if it doesn't exist
                                    if (!assistantMessageDiv) {
                                        assistantMessageDiv = document.createElement('div');
                                        assistantMessageDiv.className = 'message assistant-message';
                                        assistantMessageDiv.innerHTML = `
                                            <div class="message-header">
                                                <strong>Assistant:</strong>
                                            </div>
                                            <div class="message-content"></div>
                                        `;
                                        chatOutput.appendChild(assistantMessageDiv);
                                    }
                                    
                                    // Update the message content
                                    const messageContent = assistantMessageDiv.querySelector('.message-content');
                                    messageContent.innerHTML = marked.parse(assistantMessage);
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
                                if (!assistantMessageDiv) {
                                    assistantMessageDiv = document.createElement('div');
                                    assistantMessageDiv.className = 'message assistant-message';
                                    assistantMessageDiv.innerHTML = `
                                        <div class="message-header">
                                            <strong>Assistant:</strong>
                                        </div>
                                        <div class="message-content"></div>
                                    `;
                                    chatOutput.appendChild(assistantMessageDiv);
                                }
                                const messageContent = assistantMessageDiv.querySelector('.message-content');
                                messageContent.innerHTML = marked.parse(data);
                            }
                        }
                    }
                }
            }

            // Add assistant message to conversation history
            if (assistantMessage.trim()) {
                const assistantMessageObj = {
                    role: 'assistant',
                    content: assistantMessage
                };
                conversationHistory.push(assistantMessageObj);
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