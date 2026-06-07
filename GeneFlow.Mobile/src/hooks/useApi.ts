import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import type {
    MobileDashboardDto,
    ProjectDto,
    CreateProjectRequest,
    ExperimentSummaryDto,
    CreateExperimentRequest,
    PlateGridDto,
} from '../types/api';

// ── Dashboard ───────────────────────────────────────────────────────────────
export function useDashboard() {
    return useQuery<MobileDashboardDto>({
        queryKey: ['dashboard'],
        queryFn: () => apiClient.get('/dashboard/mobile').then(r => r.data),
        staleTime: 30_000,
    });
}

// ── Projects ────────────────────────────────────────────────────────────────
export function useProjects() {
    return useQuery<ProjectDto[]>({
        queryKey: ['projects'],
        queryFn: () => apiClient.get('/projects').then(r => r.data),
    });
}

export function useCreateProject() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (data: CreateProjectRequest) =>
            apiClient.post<ProjectDto>('/projects', data).then(r => r.data),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['projects'] });
        },
    });
}

export function useDeleteProject() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (projectId: string) => apiClient.delete(`/projects/${projectId}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['projects'] });
            qc.invalidateQueries({ queryKey: ['experiments'] });
        },
    });
}

// ── Experiments ─────────────────────────────────────────────────────────────
export function useExperiments() {
    return useQuery<ExperimentSummaryDto[]>({
        queryKey: ['experiments'],
        queryFn: () => apiClient.get('/experiments').then(r => r.data),
    });
}

export function useProjectExperiments(projectId: string | undefined) {
    return useQuery<ExperimentSummaryDto[]>({
        queryKey: ['experiments', 'project', projectId],
        queryFn: () => apiClient.get(`/projects/${projectId}/experiments`).then(r => r.data),
        enabled: !!projectId,
    });
}

export function useCreateExperiment() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (data: CreateExperimentRequest) =>
            apiClient.post('/experiments', data).then(r => r.data),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['experiments'] });
            qc.invalidateQueries({ queryKey: ['dashboard'] });
        },
    });
}

export function useDuplicateExperiment() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (experimentId: string) =>
            apiClient.post(`/experiments/${experimentId}/duplicate`).then(r => r.data),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['experiments'] });
        },
    });
}

export function useDeleteExperiment() {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (experimentId: string) => apiClient.delete(`/experiments/${experimentId}`),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['experiments'] });
            qc.invalidateQueries({ queryKey: ['dashboard'] });
        },
    });
}

// ── Plate ────────────────────────────────────────────────────────────────────
export function usePlateGrid(experimentId: string | undefined) {
    return useQuery<PlateGridDto>({
        queryKey: ['plate', experimentId],
        queryFn: () => apiClient.get(`/experiments/${experimentId}/plate`).then(r => r.data),
        enabled: !!experimentId,
    });
}

export function useUpdateWell(experimentId: string) {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: ({ wellId, data }: { wellId: string; data: any }) =>
            apiClient.put(`/experiments/${experimentId}/plate/wells/${wellId}`, data).then(r => r.data),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['plate', experimentId] });
        },
    });
}

export function useQuickFill(experimentId: string) {
    const qc = useQueryClient();
    return useMutation({
        mutationFn: (data: any) =>
            apiClient.post(`/experiments/${experimentId}/plate/quick-fill`, data).then(r => r.data),
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['plate', experimentId] });
            qc.invalidateQueries({ queryKey: ['experiments'] });
        },
    });
}
