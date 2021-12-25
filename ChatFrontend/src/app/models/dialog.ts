import { User } from "./user";

export class Dialog {
    constructor(
        public id: number,
        public lastUserId: number,
        public lastMessage: string,
        public dateTime: string,
        public login: string,
        public image: string,
        public isConfirm: boolean,
    ) {}
}