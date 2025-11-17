export function initChat(token, conversationId, currentUserId) {
    console.log("initChat RUNNING", token, conversationId, currentUserId);

    const socket = new WebSocket(`ws://localhost:8081/ws/chat?access_token=${token}`);
    const chatBox = document.getElementById("chatBox");

    socket.onopen = () => {
        console.log("WebSocket connected");

        socket.send(JSON.stringify({ protocol: "json", version: 1 }) + "\x1e");

        socket.send(JSON.stringify({
            type: 1,
            target: "JoinConversation",
            arguments: [conversationId]
        }) + "\x1e");
    };

    socket.onmessage = (event) => {
        const messages = event.data.split("\x1e").filter(m => m.trim() !== "");
        console.log("Raw messages:", messages);

        messages.forEach(msgStr => {
            try {
                const msgObj = JSON.parse(msgStr);
                if (msgObj.type === 1 && msgObj.arguments && msgObj.arguments.length > 0) {
                    const messageData = msgObj.arguments[0];
                    const isMe = messageData.senderId === currentUserId;

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
                    senderLabel.textContent = isMe ? "You" : messageData.senderId;
                    wrapper.appendChild(senderLabel);

                    const bubble = document.createElement("div");
                    bubble.classList.add("p-2", "rounded");
                    bubble.style.maxWidth = "70%";
                    bubble.style.backgroundColor = isMe ? "#0d6efd" : "#e4e6eb";
                    bubble.style.color = isMe ? "white" : "black";
                    bubble.textContent = messageData.content;

                    wrapper.appendChild(bubble);
                    chatBox.appendChild(wrapper);

                    chatBox.scrollTop = chatBox.scrollHeight;
                }
            } catch (e) {
                console.error("Invalid message", e);
            }
        });
    };

    socket.onclose = () => console.log("WebSocket closed");
    socket.onerror = (err) => console.error("WebSocket error:", err);

    return socket;
}
