import React from 'react';
import { Redirect, Route } from 'react-router-dom';
import {
  IonApp,
  IonIcon,
  IonLabel,
  IonRouterOutlet,
  IonTabBar,
  IonTabButton,
  IonTabs,
  setupIonicReact
} from '@ionic/react';
import { IonReactRouter } from '@ionic/react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { home, folder, flask, cloudUpload, ellipsisHorizontal } from 'ionicons/icons';
import { AuthProvider, useAuth } from './context/AuthContext';

import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProjectsPage from './pages/ProjectsPage';
import ExperimentsPage from './pages/ExperimentsPage';
import UploadPage from './pages/UploadPage';
import MorePage from './pages/MorePage';

/* Core CSS required for Ionic components to work properly */
import '@ionic/react/css/core.css';

/* Basic CSS for apps built with Ionic */
import '@ionic/react/css/normalize.css';
import '@ionic/react/css/structure.css';
import '@ionic/react/css/typography.css';

/* Optional CSS utils that can be commented out */
import '@ionic/react/css/padding.css';
import '@ionic/react/css/float-elements.css';
import '@ionic/react/css/text-alignment.css';
import '@ionic/react/css/text-transformation.css';
import '@ionic/react/css/flex-utils.css';
import '@ionic/react/css/display.css';

import '@ionic/react/css/palettes/dark.system.css';

/* Theme variables */
import './theme/variables.css';

setupIonicReact();

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30000,
    },
  },
});

const AppRoutes: React.FC = () => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return (
      <IonRouterOutlet>
        <Route exact path="/login" component={LoginPage} />
        <Route render={() => <Redirect to="/login" />} />
      </IonRouterOutlet>
    );
  }

  return (
    <IonTabs>
      <IonRouterOutlet>
        <Route exact path="/home" component={DashboardPage} />
        <Route exact path="/projects" component={ProjectsPage} />
        <Route exact path="/experiments" component={ExperimentsPage} />
        <Route exact path="/upload" component={UploadPage} />
        <Route exact path="/more" component={MorePage} />
        <Route exact path="/">
          <Redirect to="/home" />
        </Route>
      </IonRouterOutlet>
      <IonTabBar slot="bottom">
        <IonTabButton tab="home" href="/home">
          <IonIcon icon={home} />
          <IonLabel>Home</IonLabel>
        </IonTabButton>
        <IonTabButton tab="projects" href="/projects">
          <IonIcon icon={folder} />
          <IonLabel>Projects</IonLabel>
        </IonTabButton>
        <IonTabButton tab="experiments" href="/experiments">
          <IonIcon icon={flask} />
          <IonLabel>Experiments</IonLabel>
        </IonTabButton>
        <IonTabButton tab="upload" href="/upload">
          <IonIcon icon={cloudUpload} />
          <IonLabel>Upload</IonLabel>
        </IonTabButton>
        <IonTabButton tab="more" href="/more">
          <IonIcon icon={ellipsisHorizontal} />
          <IonLabel>More</IonLabel>
        </IonTabButton>
      </IonTabBar>
    </IonTabs>
  );
};

const App: React.FC = () => (
  <QueryClientProvider client={queryClient}>
    <AuthProvider>
      <IonApp>
        <IonReactRouter>
          <AppRoutes />
        </IonReactRouter>
      </IonApp>
    </AuthProvider>
  </QueryClientProvider>
);

export default App;

