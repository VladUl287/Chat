export class Message{
    constructor(
        public id: number,
        public userId: number,
        public dialogId: number,
        public content: string,
        public dateCreate: string,
        public isRead: boolean,
        public isSelected: boolean = false
    ) {}
}