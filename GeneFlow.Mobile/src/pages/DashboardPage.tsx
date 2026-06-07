import React from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
  IonCard, IonCardHeader, IonCardTitle, IonCardSubtitle, IonCardContent,
  IonGrid, IonRow, IonCol, IonButton, IonBadge
} from '@ionic/react';
import { useAuth } from '../context/AuthContext';

const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Lab Dashboard</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent className="ion-padding">
        <p style={{ color: 'var(--ion-color-medium)' }}>Welcome, {user?.fullName}</p>

        <IonButton expand="block" routerLink="/experiments/new" style={{ marginBottom: 16 }}>
          + New Experiment
        </IonButton>

        {/* Summary Cards */}
        <IonGrid>
          <IonRow>
            <IonCol size="6">
              <IonCard>
                <IonCardHeader>
                  <IonCardTitle>--</IonCardTitle>
                  <IonCardSubtitle>My Drafts</IonCardSubtitle>
                </IonCardHeader>
              </IonCard>
            </IonCol>
            <IonCol size="6">
              <IonCard>
                <IonCardHeader>
                  <IonCardTitle>--</IonCardTitle>
                  <IonCardSubtitle>Pending Analysis</IonCardSubtitle>
                </IonCardHeader>
              </IonCard>
            </IonCol>
            <IonCol size="6">
              <IonCard>
                <IonCardHeader>
                  <IonCardTitle color="warning">--</IonCardTitle>
                  <IonCardSubtitle>With Warnings</IonCardSubtitle>
                </IonCardHeader>
              </IonCard>
            </IonCol>
            <IonCol size="6">
              <IonCard>
                <IonCardHeader>
                  <IonCardTitle>--</IonCardTitle>
                  <IonCardSubtitle>Recent Reports</IonCardSubtitle>
                </IonCardHeader>
              </IonCard>
            </IonCol>
          </IonRow>
        </IonGrid>

        <h3 style={{ marginTop: 16 }}>Recent Experiments</h3>
        <IonCard>
          <IonCardContent>
            <p style={{ color: 'var(--ion-color-medium)', textAlign: 'center' }}>
              No recent experiments yet.
            </p>
          </IonCardContent>
        </IonCard>
      </IonContent>
    </IonPage>
  );
};

export default DashboardPage;
