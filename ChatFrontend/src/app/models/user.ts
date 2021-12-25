import { SafeUrl } from "@angular/platform-browser";

export class User {
    constructor(
        public id: number,
        public image: string,
        public login: string,
        public email: string,
        public isFriend: boolean = false
    ) {}
}