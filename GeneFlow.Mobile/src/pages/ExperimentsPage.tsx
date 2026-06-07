import React from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonCard, IonCardContent, IonButton
} from '@ionic/react';

const ExperimentsPage: React.FC = () => (
    <IonPage>
        <IonHeader>
            <IonToolbar>
                <IonTitle>My Experiments</IonTitle>
            </IonToolbar>
        </IonHeader>
        <IonContent className="ion-padding">
            <IonButton expand="block" routerLink="/experiments/new" style={{ marginBottom: 16 }}>
                + New Experiment
            </IonButton>
            <IonCard>
                <IonCardContent>
                    <p style={{ color: 'var(--ion-color-medium)', textAlign: 'center' }}>
                        No experiments yet. Create your first experiment.
                    </p>
                </IonCardContent>
            </IonCard>
        </IonContent>
    </IonPage>
);

export default ExperimentsPage;
