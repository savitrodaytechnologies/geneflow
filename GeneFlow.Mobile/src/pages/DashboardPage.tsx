import React from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonCard, IonCardHeader, IonCardTitle, IonCardContent,
    IonGrid, IonRow, IonCol, IonButton, IonBadge, IonSpinner,
    IonList, IonItem, IonLabel, IonText, IonRefresher, IonRefresherContent,
    IonIcon,
} from '@ionic/react';
import { flaskOutline, warningOutline, documentTextOutline, timeOutline, addCircleOutline } from 'ionicons/icons';
import { useAuth } from '../context/AuthContext';
import { useDashboard } from '../hooks/useApi';
import { STATUS_COLORS } from '../types/api';
import { useHistory } from 'react-router-dom';

const DashboardPage: React.FC = () => {
    const { user } = useAuth();
    const history = useHistory();
    const { data, isLoading, refetch } = useDashboard();

    const summary = data?.summary;
    const recents = data?.recentExperiments ?? [];

    const summaryCards = [
        { label: 'My Drafts', value: summary?.myDrafts ?? 0, color: 'primary', icon: flaskOutline },
        { label: 'Pending Analysis', value: summary?.pendingAnalysis ?? 0, color: 'warning', icon: timeOutline },
        { label: 'With Warnings', value: summary?.withWarnings ?? 0, color: 'danger', icon: warningOutline },
        { label: 'Recent Reports', value: summary?.recentReports ?? 0, color: 'success', icon: documentTextOutline },
    ];

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar>
                    <IonTitle>GeneFlow</IonTitle>
                </IonToolbar>
            </IonHeader>

            <IonContent>
                <IonRefresher slot="fixed" onIonRefresh={async (e) => { await refetch(); e.detail.complete(); }}>
                    <IonRefresherContent />
                </IonRefresher>

                <div style={{ padding: '16px 16px 8px' }}>
                    <IonText color="medium">
                        <p style={{ margin: 0 }}>Welcome back, <strong>{user?.fullName}</strong></p>
                        <p style={{ margin: 0, fontSize: 12 }}>{user?.labRole} · {user?.email}</p>
                    </IonText>
                </div>

                <div style={{ padding: '0 16px 16px' }}>
                    <IonButton expand="block" onClick={() => history.push('/experiments', { openNew: true })}>
                        <IonIcon slot="start" icon={addCircleOutline} />
                        New Experiment
                    </IonButton>
                </div>

                {isLoading ? (
                    <div style={{ textAlign: 'center', padding: 32 }}>
                        <IonSpinner />
                    </div>
                ) : (
                    <>
                        {/* Summary Cards */}
                        <IonGrid style={{ padding: '0 8px' }}>
                            <IonRow>
                                {summaryCards.map(card => (
                                    <IonCol size="6" key={card.label}>
                                        <IonCard style={{ margin: '4px', borderRadius: 12 }}>
                                            <IonCardContent style={{ padding: '12px 16px' }}>
                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                                    <div>
                                                        <div style={{ fontSize: 28, fontWeight: 700, color: `var(--ion-color-${card.color})` }}>
                                                            {card.value}
                                                        </div>
                                                        <div style={{ fontSize: 12, color: 'var(--ion-color-medium)' }}>{card.label}</div>
                                                    </div>
                                                    <IonIcon icon={card.icon} style={{ fontSize: 28, color: `var(--ion-color-${card.color}-shade)`, opacity: 0.4 }} />
                                                </div>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                ))}
                            </IonRow>
                        </IonGrid>

                        {/* Recent Experiments */}
                        <div style={{ padding: '16px 16px 8px' }}>
                            <IonText><h3 style={{ margin: 0, fontSize: 15, fontWeight: 600 }}>Recent Experiments</h3></IonText>
                        </div>

                        {recents.length === 0 ? (
                            <div style={{ textAlign: 'center', padding: '24px', color: 'var(--ion-color-medium)' }}>
                                <IonIcon icon={flaskOutline} style={{ fontSize: 48, marginBottom: 8 }} />
                                <p>No experiments yet. Create your first one!</p>
                            </div>
                        ) : (
                            <IonList lines="none" style={{ padding: '0 8px' }}>
                                {recents.map(exp => (
                                    <IonItem
                                        key={exp.experimentId}
                                        button
                                        onClick={() => history.push(`/experiments/${exp.experimentId}`)}
                                        style={{ '--border-radius': '12px', marginBottom: 4 }}
                                    >
                                        <IonLabel>
                                            <h2 style={{ fontWeight: 600, fontSize: 14 }}>{exp.experimentName}</h2>
                                            <p style={{ fontSize: 12 }}>{exp.projectName ?? 'No project'}</p>
                                            <p style={{ fontSize: 11, color: 'var(--ion-color-medium)' }}>
                                                {new Date(exp.lastUpdated).toLocaleDateString()}
                                            </p>
                                        </IonLabel>
                                        <div slot="end" style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 4 }}>
                                            <IonBadge color={STATUS_COLORS[exp.status] ?? 'medium'} style={{ fontSize: 10 }}>
                                                {exp.status}
                                            </IonBadge>
                                            {exp.warningCount > 0 && (
                                                <IonBadge color="warning" style={{ fontSize: 10 }}>
                                                    {exp.warningCount} ⚠
                                                </IonBadge>
                                            )}
                                        </div>
                                    </IonItem>
                                ))}
                            </IonList>
                        )}
                    </>
                )}
            </IonContent>
        </IonPage>
    );
};

export default DashboardPage;
