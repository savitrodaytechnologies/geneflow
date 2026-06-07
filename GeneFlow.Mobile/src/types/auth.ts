export interface AuthUser {
    userId: string;
    email: string;
    fullName: string;
    systemRole: string;
    labId?: string;
    labRole?: string;
}
