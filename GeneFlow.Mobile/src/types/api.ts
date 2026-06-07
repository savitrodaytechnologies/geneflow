// Shared API types for GeneFlow frontend

export interface DashboardSummaryDto {
    myDrafts: number;
    pendingAnalysis: number;
    withWarnings: number;
    recentReports: number;
}

export interface RecentExperimentDto {
    experimentId: string;
    experimentName: string;
    projectName?: string;
    status: string;
    lastUpdated: string;
    warningCount: number;
}

export interface MobileDashboardDto {
    summary: DashboardSummaryDto;
    recentExperiments: RecentExperimentDto[];
}

export interface ProjectDto {
    projectId: string;
    projectName: string;
    description?: string;
    labId: string;
    createdByUserId: string;
    createdByName: string;
    experimentCount: number;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateProjectRequest {
    projectName: string;
    description?: string;
}

export interface ExperimentSummaryDto {
    experimentId: string;
    experimentName: string;
    projectName?: string;
    ownerName: string;
    status: string;
    visibility: string;
    warningCount: number;
    lastUpdated: string;
}

export interface CreateExperimentRequest {
    projectId?: string;
    experimentName: string;
    experimentDate: string; // ISO date
    referenceGene: string;
    controlSampleName: string;
    visibility: number; // 0=Private, 1=Project, 2=Lab
    hypothesis?: string;
    objective?: string;
    instrumentName?: string;
    sampleSource?: string;
    treatmentCondition?: string;
    notes?: string;
}

export interface PlateWellDto {
    plateWellId: string;
    plateLayoutId: string;
    wellId: string;
    sampleName?: string;
    targetGene?: string;
    referenceGene?: string;
    sampleType?: string;
    replicateGroup?: string;
    ctValue?: number;
    isExcluded: boolean;
    exclusionReason?: string;
    rowIndex: number;
    colIndex: number;
}

export interface PlateGridDto {
    plateLayoutId: string;
    experimentId: string;
    layoutName: string;
    totalWells: number;
    filledWells: number;
    excludedWells: number;
    grid: PlateWellDto[][];
}

export type ExperimentStatus = 'Draft' | 'PlateDesigned' | 'DataUploaded' | 'Analyzed' | 'Finalized' | 'Archived';

export const STATUS_COLORS: Record<string, string> = {
    Draft: 'medium',
    PlateDesigned: 'primary',
    DataUploaded: 'secondary',
    Analyzed: 'success',
    Finalized: 'tertiary',
    Archived: 'dark',
};

// ── Auth / User types ───────────────────────────────────────────────────────
export interface UserDto {
    userId: string;
    email: string;
    phoneNumber?: string;
    fullName: string;
    systemRole: string;
    labId?: string;
    labRole?: string;
}

export interface LoginResponse {
    token: string;
    user: UserDto;
}

export interface RegisterLabRequest {
    labName: string;
    institutionName?: string;
    adminFullName: string;
    adminEmail: string;
    adminPhoneNumber?: string;
    adminPassword: string;
}

export interface RegisterLabResponse {
    labId: string;
    labName: string;
    token: string;
    user: UserDto;
}

export interface LabMemberDto {
    userId: string;
    fullName: string;
    email: string;
    phoneNumber?: string;
    labRole: string;
    isActive: boolean;
}

export interface AddLabUserRequest {
    fullName: string;
    email?: string;
    phoneNumber?: string;
    password: string;
    labRole: string;
}

export interface ForgotPasswordRequest {
    email?: string;
    phoneNumber?: string;
}

export interface ForgotPasswordResponse {
    resetCode: string;
    message: string;
}

export interface ResetPasswordRequest {
    email?: string;
    phoneNumber?: string;
    resetCode: string;
    newPassword: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}
