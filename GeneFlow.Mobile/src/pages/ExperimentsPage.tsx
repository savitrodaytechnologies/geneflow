import React, { useState, useEffect } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonButton, IonSpinner, IonList, IonItem, IonLabel, IonText,
    IonBadge, IonFab, IonFabButton, IonIcon,
    IonModal, IonInput, IonSelect, IonSelectOption,
    IonButtons, IonRefresher, IonRefresherContent,
    IonChip, IonSearchbar,
    useIonAlert, useIonToast,
} from '@ionic/react';
import { add, flaskOutline, copyOutline, trashOutline } from 'ionicons/icons';
import { useExperiments, useCreateExperiment, useDuplicateExperiment, useDeleteExperiment, useProjects } from '../hooks/useApi';
import { STATUS_COLORS } from '../types/api';
import type { CreateExperimentRequest } from '../types/api';
import { useIonRouter } from '@ionic/react';
import { useLocation } from 'react-router-dom';

const STATUS_FILTERS = ['All', 'Draft', 'PlateDesigned', 'DataUploaded', 'Analyzed', 'Finalized'];

const ExperimentsPage: React.FC = () => {
    const router = useIonRouter();
    const location = useLocation<{ openNew?: boolean }>();
    const { data: experiments, isLoading, refetch } = useExperiments();
    const { data: projects } = useProjects();
    const createExperiment = useCreateExperiment();
    const duplicateExperiment = useDuplicateExperiment();
    const deleteExperiment = useDeleteExperiment();
    const [presentAlert] = useIonAlert();
    const [presentToast] = useIonToast();

    const [showModal, setShowModal] = useState(false);
    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState('All');

    // Open create modal when Dashboard "New Experiment" button pushes state { openNew: true }
    useEffect(() => {
        if ((location.state as any)?.openNew) {
            setForm({ experimentName: '', experimentDate: new Date().toISOString().split('T')[0], referenceGene: 'GAPDH', controlSampleName: 'Control', visibility: 2 });
            setShowModal(true);
            // Clear state so pressing back doesn't re-open the modal
            window.history.replaceState({}, '');
        }
    }, [location.state]);

    // Form state
    const [form, setForm] = useState<Partial<CreateExperimentRequest>>({
        experimentName: '',
        experimentDate: new Date().toISOString().split('T')[0],
        referenceGene: 'GAPDH',
        controlSampleName: 'Control',
        visibility: 2,
    });

    const filtered = (experiments ?? []).filter(exp => {
        const matchSearch = !search || exp.experimentName.toLowerCase().includes(search.toLowerCase());
        const matchStatus = statusFilter === 'All' || exp.status === statusFilter;
        return matchSearch && matchStatus;
    });

    const handleCreate = async () => {
        if (!form.experimentName?.trim() || !form.referenceGene?.trim()) return;
        try {
            const created = await createExperiment.mutateAsync(form as CreateExperimentRequest);
            setShowModal(false);
            setForm({ experimentName: '', experimentDate: new Date().toISOString().split('T')[0], referenceGene: 'GAPDH', controlSampleName: 'Control', visibility: 2 });
            presentToast({ message: 'Experiment created with 96 wells!', duration: 2500, color: 'success' });
            router.push(`/experiments/${(created as any).experimentId}`);
        } catch {
            presentToast({ message: 'Failed to create experiment.', duration: 3000, color: 'danger' });
        }
    };

    const handleModalDismiss = () => {
        setShowModal(false);
    };

    const handleDuplicate = async (expId: string, expName: string) => {
        try {
            await duplicateExperiment.mutateAsync(expId);
            presentToast({ message: `Duplicated "${expName}"`, duration: 2000, color: 'success' });
        } catch {
            presentToast({ message: 'Failed to duplicate.', duration: 3000, color: 'danger' });
        }
    };

    const handleDelete = (expId: string, expName: string) => {
        presentAlert({
            header: 'Delete Experiment',
            message: `Delete "${expName}"?`,
            buttons: [
                { text: 'Cancel', role: 'cancel' },
                {
                    text: 'Delete', role: 'destructive',
                    handler: async () => {
                        try {
                            await deleteExperiment.mutateAsync(expId);
                            presentToast({ message: 'Deleted.', duration: 1500, color: 'medium' });
                        } catch {
                            presentToast({ message: 'Failed to delete.', duration: 3000, color: 'danger' });
                        }
                    }
                }
            ],
        });
    };

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar>
                    <IonTitle>Experiments</IonTitle>
                </IonToolbar>
            </IonHeader>

            <IonContent>
                <IonRefresher slot="fixed" onIonRefresh={async e => { await refetch(); e.detail.complete(); }}>
                    <IonRefresherContent />
                </IonRefresher>

                <IonSearchbar
                    value={search}
                    onIonInput={e => setSearch(e.detail.value ?? '')}
                    placeholder="Search experiments..."
                    style={{ padding: '8px 8px 0' }}
                />

                {/* Status filter chips */}
                <div style={{ display: 'flex', gap: 4, padding: '4px 12px 8px', overflowX: 'auto' }}>
                    {STATUS_FILTERS.map(s => (
                        <IonChip
                            key={s}
                            color={statusFilter === s ? 'primary' : 'medium'}
                            outline={statusFilter !== s}
                            onClick={() => setStatusFilter(s)}
                            style={{ flexShrink: 0 }}
                        >
                            {s}
                        </IonChip>
                    ))}
                </div>

                {isLoading ? (
                    <div style={{ textAlign: 'center', padding: 32 }}><IonSpinner /></div>
                ) : !filtered.length ? (
                    <div style={{ textAlign: 'center', padding: '40px 24px', color: 'var(--ion-color-medium)' }}>
                        <IonIcon icon={flaskOutline} style={{ fontSize: 56, marginBottom: 12 }} />
                        <p>{experiments?.length === 0 ? 'No experiments yet.' : 'No experiments match your filter.'}</p>
                    </div>
                ) : (
                    <IonList lines="none" style={{ padding: '0 8px' }}>
                        {filtered.map(exp => (
                            <IonItem
                                key={exp.experimentId}
                                button
                                onClick={() => router.push(`/experiments/${exp.experimentId}`)}
                                style={{ '--border-radius': '12px', marginBottom: 6 }}
                            >
                                <IonIcon icon={flaskOutline} slot="start" color={STATUS_COLORS[exp.status] ?? 'medium'} />
                                <IonLabel>
                                    <h2 style={{ fontWeight: 600, fontSize: 14 }}>{exp.experimentName}</h2>
                                    <p style={{ fontSize: 12 }}>{exp.projectName ?? 'No project'} · {exp.ownerName}</p>
                                    <p style={{ fontSize: 11, color: 'var(--ion-color-medium)' }}>
                                        {new Date(exp.lastUpdated).toLocaleDateString()}
                                        {exp.warningCount > 0 && ` · ${exp.warningCount} ⚠️`}
                                    </p>
                                </IonLabel>
                                <div slot="end" style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 4 }}>
                                    <IonBadge color={STATUS_COLORS[exp.status] ?? 'medium'} style={{ fontSize: 10 }}>
                                        {exp.status}
                                    </IonBadge>
                                    <div style={{ display: 'flex', gap: 2 }}>
                                        <IonButton
                                            fill="clear" size="small" color="secondary"
                                            onClick={e => { e.stopPropagation(); e.preventDefault(); handleDuplicate(exp.experimentId, exp.experimentName); }}
                                        >
                                            <IonIcon icon={copyOutline} />
                                        </IonButton>
                                        <IonButton
                                            fill="clear" size="small" color="danger"
                                            onClick={e => { e.stopPropagation(); e.preventDefault(); handleDelete(exp.experimentId, exp.experimentName); }}
                                        >
                                            <IonIcon icon={trashOutline} />
                                        </IonButton>
                                    </div>
                                </div>
                            </IonItem>
                        ))}
                    </IonList>
                )}

                <IonFab vertical="bottom" horizontal="end" slot="fixed">
                    <IonFabButton onClick={() => setShowModal(true)}>
                        <IonIcon icon={add} />
                    </IonFabButton>
                </IonFab>
            </IonContent>

            {/* Create Experiment Modal */}
            <IonModal isOpen={showModal} onDidDismiss={handleModalDismiss} breakpoints={[0, 0.9]} initialBreakpoint={0.9}>
                <IonHeader>
                    <IonToolbar>
                        <IonTitle>New Experiment</IonTitle>
                        <IonButtons slot="end">
                            <IonButton onClick={handleModalDismiss}>Cancel</IonButton>
                        </IonButtons>
                    </IonToolbar>
                </IonHeader>
                <IonContent className="ion-padding">
                    <IonItem>
                        <IonLabel position="stacked">Experiment Name *</IonLabel>
                        <IonInput
                            value={form.experimentName}
                            onIonInput={e => setForm(f => ({ ...f, experimentName: e.detail.value ?? '' }))}
                            placeholder="e.g. CCR2_Run_1"
                            clearInput
                        />
                    </IonItem>
                    <IonItem>
                        <IonLabel position="stacked">Date *</IonLabel>
                        <IonInput
                            type="date"
                            value={form.experimentDate}
                            onIonInput={e => setForm(f => ({ ...f, experimentDate: e.detail.value ?? '' }))}
                        />
                    </IonItem>
                    <IonItem>
                        <IonLabel position="stacked">Reference Gene *</IonLabel>
                        <IonInput
                            value={form.referenceGene}
                            onIonInput={e => setForm(f => ({ ...f, referenceGene: e.detail.value ?? '' }))}
                            placeholder="e.g. GAPDH, ACTB"
                            clearInput
                        />
                    </IonItem>
                    <IonItem>
                        <IonLabel position="stacked">Control Sample Name *</IonLabel>
                        <IonInput
                            value={form.controlSampleName}
                            onIonInput={e => setForm(f => ({ ...f, controlSampleName: e.detail.value ?? '' }))}
                            placeholder="e.g. Control, Mock"
                            clearInput
                        />
                    </IonItem>
                    <IonItem>
                        <IonLabel position="stacked">Project (optional)</IonLabel>
                        <IonSelect
                            value={form.projectId ?? ''}
                            onIonChange={e => setForm(f => ({ ...f, projectId: e.detail.value || undefined }))}
                            placeholder="Select project"
                        >
                            <IonSelectOption value="">No project</IonSelectOption>
                            {(projects ?? []).map(p => (
                                <IonSelectOption key={p.projectId} value={p.projectId}>{p.projectName}</IonSelectOption>
                            ))}
                        </IonSelect>
                    </IonItem>
                    <IonItem>
                        <IonLabel position="stacked">Visibility</IonLabel>
                        <IonSelect
                            value={form.visibility ?? 2}
                            onIonChange={e => setForm(f => ({ ...f, visibility: e.detail.value }))}
                        >
                            <IonSelectOption value={0}>Private (only me)</IonSelectOption>
                            <IonSelectOption value={1}>Project members</IonSelectOption>
                            <IonSelectOption value={2}>All lab members</IonSelectOption>
                        </IonSelect>
                    </IonItem>

                    <IonButton
                        expand="block"
                        style={{ marginTop: 24 }}
                        disabled={!form.experimentName?.trim() || !form.referenceGene?.trim() || createExperiment.isPending}
                        onClick={handleCreate}
                    >
                        {createExperiment.isPending ? <IonSpinner name="crescent" /> : 'Create Experiment (96 wells auto-added)'}
                    </IonButton>
                </IonContent>
            </IonModal>
        </IonPage>
    );
};

export default ExperimentsPage;
