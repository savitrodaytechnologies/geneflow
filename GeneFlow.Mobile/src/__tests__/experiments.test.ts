import { describe, it, expect } from 'vitest';
import type { ExperimentSummaryDto } from '../types/api';

/**
 * Tests for experiment-related business logic (client-side filtering & mapping).
 * These run without a real API — they test the same logic used in ExperimentsPage.
 */

const makeExp = (overrides: Partial<ExperimentSummaryDto> = {}): ExperimentSummaryDto => ({
    experimentId: 'exp-001',
    experimentName: 'CCR2_Run_1',
    projectName: 'Project Alpha',
    ownerName: 'Alice',
    status: 'Draft',
    visibility: 'Lab',
    warningCount: 0,
    lastUpdated: '2026-06-01T10:00:00Z',
    ...overrides,
});

// ─── Filtering logic ──────────────────────────────────────────────────────────

describe('Experiment list filtering', () => {
    const experiments: ExperimentSummaryDto[] = [
        makeExp({ experimentId: '1', experimentName: 'CCR2_Run_1', status: 'Draft' }),
        makeExp({ experimentId: '2', experimentName: 'GAPDH_Run_2', status: 'Analyzed' }),
        makeExp({ experimentId: '3', experimentName: 'CCR2_Run_3', status: 'Draft' }),
    ];

    const filterFn = (list: ExperimentSummaryDto[], search: string, statusFilter: string) =>
        list.filter(exp => {
            const matchSearch = !search || exp.experimentName.toLowerCase().includes(search.toLowerCase());
            const matchStatus = statusFilter === 'All' || exp.status === statusFilter;
            return matchSearch && matchStatus;
        });

    it('returns all experiments when filter is All and no search', () => {
        const result = filterFn(experiments, '', 'All');
        expect(result).toHaveLength(3);
    });

    it('filters by search text (case-insensitive)', () => {
        const result = filterFn(experiments, 'ccr2', 'All');
        expect(result).toHaveLength(2);
        expect(result.every(e => e.experimentName.toLowerCase().includes('ccr2'))).toBe(true);
    });

    it('filters by status', () => {
        const result = filterFn(experiments, '', 'Draft');
        expect(result).toHaveLength(2);
        expect(result.every(e => e.status === 'Draft')).toBe(true);
    });

    it('combines search and status filter', () => {
        const result = filterFn(experiments, 'ccr2', 'Analyzed');
        expect(result).toHaveLength(0); // No CCR2 experiments with Analyzed status
    });

    it('returns empty array when no match', () => {
        const result = filterFn(experiments, 'NONEXISTENT', 'All');
        expect(result).toHaveLength(0);
    });
});

// ─── Status color mapping ─────────────────────────────────────────────────────

describe('STATUS_COLORS mapping', () => {
    // Replicate the mapping from types/api.ts
    const STATUS_COLORS: Record<string, string> = {
        Draft: 'medium',
        PlateDesigned: 'secondary',
        DataUploaded: 'tertiary',
        Analyzed: 'primary',
        Finalized: 'success',
    };

    it('Draft maps to medium', () => {
        expect(STATUS_COLORS['Draft']).toBe('medium');
    });

    it('Finalized maps to success', () => {
        expect(STATUS_COLORS['Finalized']).toBe('success');
    });

    it('unknown status falls back to medium via ?? operator', () => {
        const color = STATUS_COLORS['Unknown'] ?? 'medium';
        expect(color).toBe('medium');
    });
});

// ─── Experiment name validation ───────────────────────────────────────────────

describe('Experiment name validation', () => {
    const isValid = (name: string) => name.trim().length > 0;

    it('rejects empty name', () => {
        expect(isValid('')).toBe(false);
    });

    it('rejects whitespace-only name', () => {
        expect(isValid('   ')).toBe(false);
    });

    it('accepts normal name', () => {
        expect(isValid('CCR2_Run_1')).toBe(true);
    });

    it('trims before validating', () => {
        expect(isValid('  My Experiment  ')).toBe(true);
    });
});
