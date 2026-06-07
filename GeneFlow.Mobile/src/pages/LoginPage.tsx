import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonButton, IonText, IonSpinner, IonSegment, IonSegmentButton, IonLabel,
} from '@ionic/react';
import { useHistory } from 'react-router-dom';
import apiClient from '../api/apiClient';
import { useAuth } from '../context/AuthContext';
import type { AuthUser } from '../types/auth';

interface LoginResponse {
    token: string;
    user: AuthUser;
}

// Reusable stacked-label field — avoids Ionic floating-label overlap bug
const Field: React.FC<{
    label: string;
    type?: string;
    value: string;
    onChange: (v: string) => void;
    placeholder?: string;
    onEnter?: () => void;
    inputMode?: 'text' | 'email' | 'numeric' | 'tel';
    autocomplete?: string;
}> = ({ label, type = 'text', value, onChange, placeholder, onEnter, inputMode, autocomplete }) => (
    <div style={{ marginBottom: 16 }}>
        <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 6, paddingLeft: 2 }}>
            {label}
        </label>
        <input
            type={type}
            value={value}
            onChange={e => onChange(e.target.value)}
            onKeyUp={e => { if (e.key === 'Enter' && onEnter) onEnter(); }}
            placeholder={placeholder}
            inputMode={inputMode}
            autoComplete={autocomplete}
            style={{
                width: '100%', boxSizing: 'border-box',
                padding: '12px 14px', fontSize: 16,
                borderRadius: 10, border: '1.5px solid var(--ion-color-light-shade)',
                background: 'var(--ion-item-background, var(--ion-background-color))',
                color: 'var(--ion-text-color)',
                outline: 'none',
            }}
        />
    </div>
);

const LoginPage: React.FC = () => {
    const [loginMode, setLoginMode] = useState<'phone' | 'email'>('phone');
    const [phone, setPhone] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const history = useHistory();

    // Strip all non-digits for phone — no country code needed at login
    const normalizePhone = (v: string) => v.replace(/\D/g, '');

    const handleLogin = async () => {
        if (!password) { setError('Password is required.'); return; }
        if (loginMode === 'phone' && !phone.trim()) { setError('Phone number is required.'); return; }
        if (loginMode === 'email' && !email.trim()) { setError('Email is required.'); return; }
        setError('');
        setLoading(true);
        try {
            const payload = loginMode === 'phone'
                ? { phoneNumber: normalizePhone(phone), password }
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
                        style={{ marginBottom: 20 }}
                    >
                        <IonSegmentButton value="phone"><IonLabel>Phone Number</IonLabel></IonSegmentButton>
                        <IonSegmentButton value="email"><IonLabel>Email</IonLabel></IonSegmentButton>
                    </IonSegment>

                    {loginMode === 'phone' ? (
                        <Field
                            label="Phone Number"
                            type="tel"
                            inputMode="numeric"
                            value={phone}
                            onChange={setPhone}
                            placeholder="9876543210  (digits only, no country code)"
                            autocomplete="tel-national"
                        />
                    ) : (
                        <Field
                            label="Email"
                            type="email"
                            inputMode="email"
                            value={email}
                            onChange={setEmail}
                            placeholder="you@example.com"
                            autocomplete="email"
                        />
                    )}

                    <Field
                        label="Password"
                        type="password"
                        value={password}
                        onChange={setPassword}
                        autocomplete="current-password"
                        onEnter={handleLogin}
                    />

                    {error && (
                        <IonText color="danger">
                            <p style={{ marginTop: -8, marginBottom: 12, fontSize: 13 }}>{error}</p>
                        </IonText>
                    )}

                    <IonButton expand="block" onClick={handleLogin} disabled={loading}>
                        {loading ? <IonSpinner name="crescent" /> : 'Sign In'}
                    </IonButton>

                    <div style={{ textAlign: 'center', marginTop: 20 }}>
                        <IonText color="medium" style={{ fontSize: 14 }}>New to GeneFlow?&nbsp;</IonText>
                        <IonButton fill="clear" size="small" routerLink="/register-lab">
                            Register your lab
                        </IonButton>
                    </div>
                </div>
            </IonContent>
        </IonPage>
    );
};

export default LoginPage;

