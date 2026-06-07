import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonItem, IonLabel, IonInput, IonButton, IonText, IonSpinner
} from '@ionic/react';
import { useHistory } from 'react-router-dom';
import apiClient from '../api/apiClient';
import { useAuth } from '../context/AuthContext';
import type { AuthUser } from '../types/auth';

interface LoginResponse {
    token: string;
    user: AuthUser;
}

const LoginPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const history = useHistory();

    const handleLogin = async () => {
        if (!email.trim() || !password) {
            setError('Email and password are required.');
            return;
        }
        setError('');
        setLoading(true);
        try {
            const response = await apiClient.post<LoginResponse>('/api/auth/login', { email, password });
            login(response.data.token, response.data.user);
            history.replace('/home');
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setError(axiosErr.response?.data?.message ?? 'Invalid email or password.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar>
                    <IonTitle>GeneFlow</IonTitle>
                </IonToolbar>
            </IonHeader>
            <IonContent className="ion-padding">
                <div style={{ maxWidth: 400, margin: '40px auto' }}>
                    <h2 style={{ textAlign: 'center', marginBottom: 32 }}>Sign In</h2>
                    <IonItem>
                        <IonLabel position="floating">Email</IonLabel>
                        <IonInput
                            type="email"
                            value={email}
                            onIonInput={(e) => setEmail(e.detail.value ?? '')}
                            autocomplete="email"
                        />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Password</IonLabel>
                        <IonInput
                            type="password"
                            value={password}
                            onIonInput={(e) => setPassword(e.detail.value ?? '')}
                            onKeyUp={(e) => { if (e.key === 'Enter') handleLogin(); }}
                        />
                    </IonItem>
                    {error && (
                        <IonText color="danger">
                            <p style={{ marginTop: 8, paddingLeft: 16 }}>{error}</p>
                        </IonText>
                    )}
                    <IonButton
                        expand="block"
                        style={{ marginTop: 24 }}
                        onClick={handleLogin}
                        disabled={loading}
                    >
                        {loading ? <IonSpinner name="crescent" /> : 'Sign In'}
                    </IonButton>
                </div>
            </IonContent>
        </IonPage>
    );
};

export default LoginPage;
