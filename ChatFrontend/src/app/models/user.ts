export class User {
    constructor(
        public id: number,
        public image: string,
        public login: string,
        public email: string,
        public isFriend: boolean = false,
        public isSender: boolean = false,
        public isReceiver: boolean = false
    ) {}
}