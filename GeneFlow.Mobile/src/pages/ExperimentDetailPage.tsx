import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent, IonBackButton,
    IonButtons, IonButton, IonSpinner, IonBadge, IonSegment, IonSegmentButton,
    IonLabel, IonIcon, IonCard, IonCardContent, IonItem, IonText,
    useIonToast,
} from '@ionic/react';
import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../api/apiClient';
import { gridOutline, cloudUploadOutline, statsChartOutline, documentTextOutline } from 'ionicons/icons';
import { STATUS_COLORS } from '../types/api';
import PlateSetupTab from '../components/PlateSetupTab';

const ExperimentDetailPage: React.FC = () => {
    const { experimentId } = useParams<{ experimentId: string }>();
    const [tab, setTab] = useState<string>('setup');
    const [presentToast] = useIonToast();

    const { data: experiment, isLoading } = useQuery({
        queryKey: ['experiment', experimentId],
        queryFn: () => apiClient.get(`/experiments/${experimentId}`).then(r => r.data),
        enabled: !!experimentId,
    });

    if (isLoading) {
        return (
            <IonPage>
                <IonHeader>
                    <IonToolbar>
                        <IonButtons slot="start"><IonBackButton defaultHref="/experiments" /></IonButtons>
                        <IonTitle>Loading...</IonTitle>
                    </IonToolbar>
                </IonHeader>
                <IonContent><div style={{ textAlign: 'center', padding: 40 }}><IonSpinner /></div></IonContent>
            </IonPage>
        );
    }

    if (!experiment) {
        return (
            <IonPage>
                <IonHeader>
                    <IonToolbar>
                        <IonButtons slot="start"><IonBackButton defaultHref="/experiments" /></IonButtons>
                        <IonTitle>Not Found</IonTitle>
                    </IonToolbar>
                </IonHeader>
                <IonContent><div style={{ textAlign: 'center', padding: 40 }}>Experiment not found.</div></IonContent>
            </IonPage>
        );
    }

    const statusColor = STATUS_COLORS[experiment.status] ?? 'medium';

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar>
                    <IonButtons slot="start"><IonBackButton defaultHref="/experiments" /></IonButtons>
                    <IonTitle style={{ fontSize: 15 }}>{experiment.experimentName}</IonTitle>
                    <IonButtons slot="end">
                        <IonBadge color={statusColor} style={{ marginRight: 12, fontSize: 10 }}>
                            {experiment.status}
                        </IonBadge>
                    </IonButtons>
                </IonToolbar>
            </IonHeader>

            <IonContent>
                {/* Experiment Info Card */}
                <IonCard style={{ margin: '12px 12px 0', borderRadius: 12 }}>
                    <IonCardContent style={{ padding: '12px 16px' }}>
                        <IonItem lines="none" style={{ '--padding-start': 0, '--inner-padding-end': 0 }}>
                            <IonLabel>
                                <div style={{ fontSize: 12, color: 'var(--ion-color-medium)' }}>Reference Gene</div>
                                <div style={{ fontWeight: 600 }}>{experiment.referenceGene}</div>
                            </IonLabel>
                            <IonLabel>
                                <div style={{ fontSize: 12, color: 'var(--ion-color-medium)' }}>Control</div>
                                <div style={{ fontWeight: 600 }}>{experiment.controlSampleName}</div>
                            </IonLabel>
                            <IonLabel>
                                <div style={{ fontSize: 12, color: 'var(--ion-color-medium)' }}>Date</div>
                                <div style={{ fontWeight: 600 }}>{experiment.experimentDate?.split('T')[0]}</div>
                            </IonLabel>
                        </IonItem>
                    </IonCardContent>
                </IonCard>

                {/* Tab Navigation */}
                <IonSegment
                    value={tab}
                    onIonChange={e => setTab(e.detail.value as string)}
                    style={{ padding: '8px 12px 0' }}
                    scrollable
                >
                    <IonSegmentButton value="setup">
                        <IonIcon icon={gridOutline} />
                        <IonLabel>Plate</IonLabel>
                    </IonSegmentButton>
                    <IonSegmentButton value="upload">
                        <IonIcon icon={cloudUploadOutline} />
                        <IonLabel>Ct Data</IonLabel>
                    </IonSegmentButton>
                    <IonSegmentButton value="analysis">
                        <IonIcon icon={statsChartOutline} />
                        <IonLabel>Analysis</IonLabel>
                    </IonSegmentButton>
                    <IonSegmentButton value="notes">
                        <IonIcon icon={documentTextOutline} />
                        <IonLabel>Notes</IonLabel>
                    </IonSegmentButton>
                </IonSegment>

                {/* Tab Content */}
                {tab === 'setup' && <PlateSetupTab experimentId={experimentId} experiment={experiment} />}
                {tab === 'upload' && (
                    <div style={{ padding: '24px', textAlign: 'center', color: 'var(--ion-color-medium)' }}>
                        <IonIcon icon={cloudUploadOutline} style={{ fontSize: 56, marginBottom: 12 }} />
                        <p>Ct data upload coming in Week 2.</p>
                        <p style={{ fontSize: 12 }}>CSV upload for raw Ct values from qPCR instrument</p>
                    </div>
                )}
                {tab === 'analysis' && (
                    <div style={{ padding: '24px', textAlign: 'center', color: 'var(--ion-color-medium)' }}>
                        <IonIcon icon={statsChartOutline} style={{ fontSize: 56, marginBottom: 12 }} />
                        <p>ΔΔCt analysis engine coming in Week 2.</p>
                    </div>
                )}
                {tab === 'notes' && (
                    <div style={{ padding: '24px', textAlign: 'center', color: 'var(--ion-color-medium)' }}>
                        <IonIcon icon={documentTextOutline} style={{ fontSize: 56, marginBottom: 12 }} />
                        <p>Experiment notes coming in Week 2.</p>
                    </div>
                )}
            </IonContent>
        </IonPage>
    );
};

export default ExperimentDetailPage;
