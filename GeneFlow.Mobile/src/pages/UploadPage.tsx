import React from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent, IonCard, IonCardContent
} from '@ionic/react';

const UploadPage: React.FC = () => (
    <IonPage>
        <IonHeader>
            <IonToolbar>
                <IonTitle>Upload</IonTitle>
            </IonToolbar>
        </IonHeader>
        <IonContent className="ion-padding">
            <IonCard>
                <IonCardContent>
                    <p style={{ color: 'var(--ion-color-medium)', textAlign: 'center' }}>
                        Open an experiment to upload Ct CSV data.
                    </p>
                </IonCardContent>
            </IonCard>
        </IonContent>
    </IonPage>
);

export default UploadPage;
