import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonItem, IonLabel, IonInput, IonButton, IonText, IonSpinner,
    IonSegment, IonSegmentButton,
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
    const [loginMode, setLoginMode] = useState<'phone' | 'email'>('phone');
    const [phone, setPhone] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const history = useHistory();

    const handleLogin = async () => {
        if (!password) { setError('Password is required.'); return; }
        if (loginMode === 'phone' && !phone.trim()) { setError('Phone number is required.'); return; }
        if (loginMode === 'email' && !email.trim()) { setError('Email is required.'); return; }
        setError('');
        setLoading(true);
        try {
            const payload = loginMode === 'phone'
                ? { phoneNumber: phone.trim(), password }
                : { email: email.trim(), password };
            const response = await apiClient.post<LoginResponse>('/auth/login', payload);
            login(response.data.token, response.data.user);
            history.replace('/home');
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setError(axiosErr.response?.data?.message ?? 'Invalid credentials.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar color="primary">
                    <IonTitle>GeneFlow</IonTitle>
                </IonToolbar>
            </IonHeader>
            <IonContent className="ion-padding">
                <div style={{ maxWidth: 420, margin: '32px auto' }}>
                    <div style={{ textAlign: 'center', marginBottom: 28 }}>
                        <h2 style={{ margin: 0 }}>Welcome back</h2>
                        <p style={{ color: 'var(--ion-color-medium)', marginTop: 6 }}>Sign in to your lab</p>
                    </div>

                    <IonSegment
                        value={loginMode}
                        onIonChange={e => setLoginMode(e.detail.value as 'phone' | 'email')}
                        style={{ marginBottom: 16 }}
                    >
                        <IonSegmentButton value="phone">
                            <IonLabel>Phone Number</IonLabel>
                        </IonSegmentButton>
                        <IonSegmentButton value="email">
                            <IonLabel>Email</IonLabel>
                        </IonSegmentButton>
                    </IonSegment>

                    {loginMode === 'phone' ? (
                        <IonItem>
                            <IonLabel position="floating">Phone Number</IonLabel>
                            <IonInput
                                type="tel"
                                value={phone}
                                onIonInput={e => setPhone(e.detail.value ?? '')}
                                autocomplete="tel"
                                placeholder="+1 234 567 8900"
                            />
                        </IonItem>
                    ) : (
                        <IonItem>
                            <IonLabel position="floating">Email</IonLabel>
                            <IonInput
                                type="email"
                                value={email}
                                onIonInput={e => setEmail(e.detail.value ?? '')}
                                autocomplete="email"
                            />
                        </IonItem>
                    )}

                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Password</IonLabel>
                        <IonInput
                            type="password"
                            value={password}
                            onIonInput={e => setPassword(e.detail.value ?? '')}
                            onKeyUp={e => { if (e.key === 'Enter') handleLogin(); }}
                        />
                    </IonItem>

                    {error && (
                        <IonText color="danger">
                            <p style={{ marginTop: 8, paddingLeft: 16, fontSize: 14 }}>{error}</p>
                        </IonText>
                    )}

                    <IonButton expand="block" style={{ marginTop: 24 }} onClick={handleLogin} disabled={loading}>
                        {loading ? <IonSpinner name="crescent" /> : 'Sign In'}
                    </IonButton>

                    <div style={{ textAlign: 'center', marginTop: 24 }}>
                        <IonText color="medium">
                            <span style={{ fontSize: 14 }}>New to GeneFlow? </span>
                        </IonText>
                        <IonButton fill="clear" size="small" routerLink="/register-lab" style={{ fontSize: 14 }}>
                            Register your lab
                        </IonButton>
                    </div>
                </div>
            </IonContent>
        </IonPage>
    );
};

export default LoginPage;

