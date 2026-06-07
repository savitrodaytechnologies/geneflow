export interface AuthUser {
    userId: string;
    email: string;
    phoneNumber?: string;
    fullName: string;
    systemRole: string;
    labId?: string;
    labRole?: string;
}
