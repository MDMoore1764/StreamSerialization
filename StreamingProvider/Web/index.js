const url = "ws://localhost:5001/stream";

const ws = new WebSocket(url);

ws.onmessage = () => console.log("lalala");
ws.onopen = () => console.log("opened");
ws.onmessage = () => console.log("message");

// const wss = new WebSocketStream(url);
// console.log(wss);
// const { readable, writable } = await wss.connection;
// const reader = readable.getReader();
// const writer = writable.getWriter();

// while (true) {
//   const { value, done } = await reader.read();
//   if (done) {
//     break;
//   }
//   const result = await process(value);
//   await writer.write(result);
// }
