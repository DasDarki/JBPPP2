const callbacks: { [key: string]: (data: any) => void } = {};
const events: { [key: string]: (...args: any[]) => void } = {};
const bridge: ExternalBridge = (window as any).external as ExternalBridge;

interface Command {
    command: string;
    rid?: string;
    args: any[];
}

interface ExternalBridge {
    sendMessage: (json: string) => void;
    receiveMessage: (cb: (json: string) => void) => void;
}

export function generateUuid() {
    let d = new Date().getTime();
    if (typeof performance !== 'undefined' && typeof performance.now === 'function') {
        d += performance.now(); //use high-precision timer if available
    }

    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        let r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}

export function onEvent(event: string, cb: (...args: any[]) => void) {
    events[event] = cb;
}

export function offEvent(event: string) {
    delete events[event];
}

export function onceEvent(event: string, cb: (...args: any[]) => void) {
    const wrapped = (...args: any[]) => {
        cb(...args);
        offEvent(event);
    };
    onEvent(event, wrapped);
}

export function execute(name: string, ...args: any[]) {
    if (bridge) {
        const command: Command = {
            command: name,
            args: args
        };
        bridge.sendMessage(JSON.stringify(command));
    }
}

export function executeWithReturn<T>(name: string, ...args: any[]): Promise<T> {
    if (bridge) {
        const rid = generateUuid();
        const command: Command = {
            command: name,
            args: args,
            rid: rid
        };

        return new Promise<T>((resolve) => {
            callbacks[rid] = (data: any) => {
                resolve(data);
            };
            bridge.sendMessage(JSON.stringify(command));
        });
    }

    return Promise.reject("Bridge not found");
}

if (bridge) {
    bridge.receiveMessage((json: string) => {
        const command: Command = JSON.parse(json);
        if (command) {
            if (command.command === 'SendResult' && command.args[0] && command.args[1]) {
                const callback = callbacks[command.args[0]];
                if (callback) {
                    callback(command.args[1]);
                    delete callbacks[command.args[0]];
                }
            }
        }
    });
}