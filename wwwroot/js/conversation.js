import { mapGuidsToUsernames } from "/js/guidToUser.js";

export function initChat(token, conversationId, currentUserId) {
    console.log("initChat RUNNING", token, conversationId, currentUserId);

    const socket = new WebSocket(`ws://localhost:8081/ws/chat?access_token=${token}`);
    const chatBox = document.getElementById("chatBox");

    // Helper append message
    async function appendMessage(messageData) {

        const isMe = messageData.senderId === currentUserId;
        console.log(messageData.senderId + " " + currentUserId);
        const wrapper = document.createElement("div");
        wrapper.style.display = "flex";
        wrapper.style.flexDirection = "column";
        wrapper.style.alignItems = isMe ? "flex-end" : "flex-start";
        wrapper.style.marginBottom = "10px";

        const senderLabel = document.createElement("div");
        senderLabel.style.fontSize = "12px";
        senderLabel.style.fontWeight = "bold";
        senderLabel.style.marginBottom = "3px";
        senderLabel.style.color = isMe ? "#0d6efd" : "#333";
        // Map guid → username
        const usernameMap = await mapGuidsToUsernames([messageData.senderId]);
        senderLabel.textContent = isMe ? "You" : usernameMap[messageData.senderId] || messageData.senderId;
        wrapper.appendChild(senderLabel);

        const bubble = document.createElement("div");
        bubble.classList.add("p-2", "rounded");
        bubble.style.maxWidth = "70%";
        bubble.style.backgroundColor = isMe ? "#0d6efd" : "#e4e6eb";
        bubble.style.color = isMe ? "white" : "black";
        bubble.textContent = messageData.content;
        wrapper.appendChild(bubble);

        // --- Timestamp ---
        if (messageData.sentAt) { // đảm bảo có trường thời gian
            const timeLabel = document.createElement("div");
            timeLabel.style.fontSize = "10px";
            timeLabel.style.color = "#666";
            timeLabel.style.marginTop = "2px";
            timeLabel.textContent = new Date(messageData.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            wrapper.appendChild(timeLabel);
        }

        chatBox.appendChild(wrapper);
        chatBox.scrollTop = chatBox.scrollHeight;
    }

    socket.onopen = () => {
        console.log("WebSocket connected");

        // SignalR handshake
        socket.send(JSON.stringify({ protocol: "json", version: 1 }) + "\x1e");

        // Join conversation
        socket.send(JSON.stringify({
            type: 1,
            target: "JoinConversation",
            arguments: [conversationId]
        }) + "\x1e");
    };

    socket.onmessage = async (event) => {
        const messages = event.data.split("\x1e").filter(m => m.trim() !== "");
        console.log("Raw messages:", messages);

        for (const msgStr of messages) {
            try {
                const msgObj = JSON.parse(msgStr);
                if (msgObj.type === 1 && msgObj.arguments && msgObj.arguments.length > 0) {
                    let messageData = msgObj.arguments[0];

                    // Map senderId -> tên user
                    const usernameMap = await mapGuidsToUsernames([messageData.senderId]);
                    messageData.senderId = usernameMap[messageData.senderId] || messageData.senderId;

                    appendMessage(messageData);
                }
            } catch (e) {
                console.error("Invalid message", e);
            }
        }
    };

    socket.onclose = () => console.log("WebSocket closed");
    socket.onerror = (err) => console.error("WebSocket error:", err);

    return socket;
}
