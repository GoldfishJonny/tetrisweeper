mergeInto(LibraryManager.library, {
  WSInit: function(urlPtr) {
    const url = UTF8ToString(urlPtr);
    window._ws = new WebSocket(url);
    window._ws._outQueue = [];
    window._ws._inQueue  = [];

    window._ws.onopen = () => {
      console.log("[WS] open, flushing", window._ws._outQueue.length);
      window._ws._outQueue.forEach(m => window._ws.send(m));
      window._ws._outQueue = [];
    };
    window._ws.onerror  = e => console.error("[WS] error", e);
    window._ws.onclose  = () => console.warn("[WS] closed");
    window._ws.onmessage = e => {
      try {
        const msg = JSON.parse(e.data);
        window._ws._inQueue.push(msg);
      } catch(err) {
        console.warn("[WS] invalid JSON message", e.data);
      }
    };
  },

  WSSend: function(jsonPtr) {
    if (!window._ws) { console.error("[WS] not init"); return; }
    const raw = UTF8ToString(jsonPtr);
    let msg;
    try {
      const payload = JSON.parse(raw);
      msg = JSON.stringify({ type: "state", payload });
    } catch(err) {
      console.error("[WS] invalid send JSON", raw);
      return;
    }
    if (window._ws.readyState === WebSocket.CONNECTING) {
      window._ws._outQueue.push(msg);
    } else if (window._ws.readyState === WebSocket.OPEN) {
      window._ws.send(msg);
    } else {
      console.error("[WS] bad state", window._ws.readyState);
    }
  },

  WSReceive: function() {
    if (!window._ws || !window._ws._inQueue) {
      // allocate an empty C-string
      const emptyPtr = _malloc(1);
      stringToUTF8("", emptyPtr, 1);
      return emptyPtr;
    }

    for (let i = 0; i < window._ws._inQueue.length; i++) {
      const m = window._ws._inQueue[i];
      if (m.type === "command") {
        const cmdStr = JSON.stringify(m.payload);
        // carve out a C-string in WASM heap
        const len = lengthBytesUTF8(cmdStr) + 1;
        const ptr = _malloc(len);
        stringToUTF8(cmdStr, ptr, len);
        window._ws._inQueue.splice(i, 1);
        return ptr;
      }
    }

    // no command ready → return empty C-string
    const emptyPtr = _malloc(1);
    stringToUTF8("", emptyPtr, 1);
    return emptyPtr;
  }
});
