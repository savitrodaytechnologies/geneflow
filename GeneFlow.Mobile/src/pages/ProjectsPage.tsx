import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonButton, IonSpinner, IonList, IonItem, IonLabel, IonText,
    IonBadge, IonFab, IonFabButton, IonIcon,
    IonModal, IonInput, IonTextarea, IonButtons, IonRefresher, IonRefresherContent,
    useIonAlert, useIonToast,
} from '@ionic/react';
import { add, folderOutline, trashOutline } from 'ionicons/icons';
import { useProjects, useCreateProject, useDeleteProject } from '../hooks/useApi';

const ProjectsPage: React.FC = () => {
    const { data: projects, isLoading, refetch } = useProjects();
    const createProject = useCreateProject();
    const deleteProject = useDeleteProject();
    const [presentAlert] = useIonAlert();
    const [presentToast] = useIonToast();

    const [showModal, setShowModal] = useState(false);
    const [name, setName] = useState('');
    const [desc, setDesc] = useState('');

    const handleCreate = async () => {
        if (!name.trim()) return;
        try {
            await createProject.mutateAsync({ projectName: name.trim(), description: desc.trim() || undefined });
            setShowModal(false);
            setName('');
            setDesc('');
            presentToast({ message: 'Project created!', duration: 2000, color: 'success' });
        } catch {
            presentToast({ message: 'Failed to create project.', duration: 3000, color: 'danger' });
        }
    };

    const handleDelete = (projectId: string, projectName: string) => {
        presentAlert({
            header: 'Delete Project',
            message: `Delete "${projectName}"? This cannot be undone.`,
            buttons: [
                { text: 'Cancel', role: 'cancel' },
                {
                    text: 'Delete', role: 'destructive', cssClass: 'ion-color-danger',
                    handler: async () => {
                        try {
                            await deleteProject.mutateAsync(projectId);
                            presentToast({ message: 'Project deleted.', duration: 2000, color: 'medium' });
                        } catch {
                            presentToast({ message: 'Failed to delete project.', duration: 3000, color: 'danger' });
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
                    <IonTitle>Projects</IonTitle>
                </IonToolbar>
            </IonHeader>

            <IonContent>
                <IonRefresher slot="fixed" onIonRefresh={async e => { await refetch(); e.detail.complete(); }}>
                    <IonRefresherContent />
                </IonRefresher>

                {isLoading ? (
                    <div style={{ textAlign: 'center', padding: 32 }}><IonSpinner /></div>
                ) : !projects?.length ? (
                    <div style={{ textAlign: 'center', padding: '48px 24px', color: 'var(--ion-color-medium)' }}>
                        <IonIcon icon={folderOutline} style={{ fontSize: 56, marginBottom: 12 }} />
                        <p>No projects yet.<br />Create one to organize your experiments.</p>
                    </div>
                ) : (
                    <IonList lines="none" style={{ padding: '8px' }}>
                        {projects.map(proj => (
                            <IonItem
                                key={proj.projectId}
                                button
                                routerLink={`/projects/${proj.projectId}`}
                                style={{ '--border-radius': '12px', marginBottom: 6 }}
                            >
                                <IonIcon icon={folderOutline} slot="start" color="primary" />
                                <IonLabel>
                                    <h2 style={{ fontWeight: 600 }}>{proj.projectName}</h2>
                                    {proj.description && <p style={{ fontSize: 12 }}>{proj.description}</p>}
                                    <p style={{ fontSize: 11, color: 'var(--ion-color-medium)' }}>
                                        {proj.experimentCount} experiment{proj.experimentCount !== 1 ? 's' : ''} · by {proj.createdByName}
                                    </p>
                                </IonLabel>
                                <IonBadge color="primary" slot="end">{proj.experimentCount}</IonBadge>
                                <IonButton
                                    slot="end"
                                    fill="clear"
                                    color="danger"
                                    onClick={e => { e.stopPropagation(); e.preventDefault(); handleDelete(proj.projectId, proj.projectName); }}
                                >
                                    <IonIcon icon={trashOutline} />
                                </IonButton>
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

            {/* Create Project Modal */}
            <IonModal isOpen={showModal} onDidDismiss={() => setShowModal(false)} breakpoints={[0, 0.6]} initialBreakpoint={0.6}>
                <IonHeader>
                    <IonToolbar>
                        <IonTitle>New Project</IonTitle>
                        <IonButtons slot="end">
                            <IonButton onClick={() => setShowModal(false)}>Cancel</IonButton>
                        </IonButtons>
                    </IonToolbar>
                </IonHeader>
                <IonContent className="ion-padding">
                    <IonItem>
                        <IonLabel position="stacked">Project Name *</IonLabel>
                        <IonInput
                            value={name}
                            onIonInput={e => setName(e.detail.value ?? '')}
                            placeholder="e.g. CCR2 Expression Study"
                            clearInput
                        />
                    </IonItem>
                    <IonItem>
                        <IonLabel position="stacked">Description</IonLabel>
                        <IonTextarea
                            value={desc}
                            onIonInput={e => setDesc(e.detail.value ?? '')}
                            placeholder="Optional description"
                            autoGrow
                        />
                    </IonItem>
                    <IonButton
                        expand="block"
                        style={{ marginTop: 24 }}
                        disabled={!name.trim() || createProject.isPending}
                        onClick={handleCreate}
                    >
                        {createProject.isPending ? <IonSpinner name="crescent" /> : 'Create Project'}
                    </IonButton>
                </IonContent>
            </IonModal>
        </IonPage>
    );
};

export default ProjectsPage;
