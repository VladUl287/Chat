export class CreateDialog {
    constructor(
        public name: string,
        public facialImage: File,
        public userId: number,
        public usersId: number[]
    ) {}
}