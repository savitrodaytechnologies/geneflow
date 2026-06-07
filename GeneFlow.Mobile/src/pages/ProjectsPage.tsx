import React from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
  IonCard, IonCardHeader, IonCardTitle, IonCardSubtitle, IonCardContent, IonButton
} from '@ionic/react';

const ProjectsPage: React.FC = () => (
  <IonPage>
    <IonHeader>
      <IonToolbar>
        <IonTitle>Projects</IonTitle>
      </IonToolbar>
    </IonHeader>
    <IonContent className="ion-padding">
      <IonButton expand="block" style={{ marginBottom: 16 }}>+ New Project</IonButton>
      <IonCard>
        <IonCardContent>
          <p style={{ color: 'var(--ion-color-medium)', textAlign: 'center' }}>
            No projects yet. Create your first project.
          </p>
        </IonCardContent>
      </IonCard>
    </IonContent>
  </IonPage>
);

export default ProjectsPage;
