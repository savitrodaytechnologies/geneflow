import React from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
  IonList, IonItem, IonLabel, IonIcon, IonButton
} from '@ionic/react';
import { logOut } from 'ionicons/icons';
import { useAuth } from '../context/AuthContext';
import { useHistory } from 'react-router-dom';

const MorePage: React.FC = () => {
  const { user, logout } = useAuth();
  const history = useHistory();

  const handleLogout = () => {
    logout();
    history.replace('/login');
  };

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>More</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent>
        <IonList>
          <IonItem>
            <IonLabel>
              <h2>{user?.fullName}</h2>
              <p>{user?.email}</p>
              <p>{user?.labRole ?? user?.systemRole}</p>
            </IonLabel>
          </IonItem>
        </IonList>
        <div className="ion-padding">
          <IonButton expand="block" color="danger" onClick={handleLogout}>
            <IonIcon slot="start" icon={logOut} />
            Sign Out
          </IonButton>
        </div>
      </IonContent>
    </IonPage>
  );
};

export default MorePage;
