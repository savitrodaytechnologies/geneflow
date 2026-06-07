import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent, IonButtons,
    IonBackButton, IonItem, IonLabel, IonInput, IonButton, IonText,
    IonSpinner, IonNote,
} from '@ionic/react';
import { useHistory } from 'react-router-dom';
import { useRegisterLab } from '../hooks/useApi';
import { useAuth } from '../context/AuthContext';

const RegisterLabPage: React.FC = () => {
    const [labName, setLabName] = useState('');
    const [institution, setInstitution] = useState('');
    const [adminName, setAdminName] = useState('');
    const [adminEmail, setAdminEmail] = useState('');
    const [adminPhone, setAdminPhone] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');

    const { login } = useAuth();
    const history = useHistory();
    const registerLab = useRegisterLab();

    const handleSubmit = async () => {
        setError('');
        if (!labName.trim()) { setError('Lab name is required.'); return; }
        if (!adminName.trim()) { setError('Your full name is required.'); return; }
        if (!adminEmail.trim()) { setError('Email is required.'); return; }
        if (password.length < 8) { setError('Password must be at least 8 characters.'); return; }
        if (password !== confirmPassword) { setError('Passwords do not match.'); return; }

        try {
            const result = await registerLab.mutateAsync({
                labName: labName.trim(),
                institutionName: institution.trim() || undefined,
                adminFullName: adminName.trim(),
                adminEmail: adminEmail.trim(),
                adminPhoneNumber: adminPhone.trim() || undefined,
                adminPassword: password,
            });
            login(result.token, result.user as any);
            history.replace('/home');
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setError(axiosErr.response?.data?.message ?? 'Registration failed. Please try again.');
        }
    };

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar color="primary">
                    <IonButtons slot="start">
                        <IonBackButton defaultHref="/login" />
                    </IonButtons>
                    <IonTitle>Register Lab</IonTitle>
                </IonToolbar>
            </IonHeader>
            <IonContent className="ion-padding">
                <div style={{ maxWidth: 480, margin: '16px auto' }}>
                    <p style={{ color: 'var(--ion-color-medium)', marginBottom: 20 }}>
                        Create your lab account. You'll be the Lab Admin and can add team members after.
                    </p>

                    <h3 style={{ marginBottom: 4 }}>Lab Details</h3>
                    <IonItem>
                        <IonLabel position="floating">Lab Name *</IonLabel>
                        <IonInput value={labName} onIonInput={e => setLabName(e.detail.value ?? '')} />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Institution / University</IonLabel>
                        <IonInput value={institution} onIonInput={e => setInstitution(e.detail.value ?? '')} />
                    </IonItem>

                    <h3 style={{ marginTop: 24, marginBottom: 4 }}>Your Account</h3>
                    <IonItem>
                        <IonLabel position="floating">Full Name *</IonLabel>
                        <IonInput value={adminName} onIonInput={e => setAdminName(e.detail.value ?? '')} />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Email *</IonLabel>
                        <IonInput type="email" value={adminEmail} onIonInput={e => setAdminEmail(e.detail.value ?? '')} autocomplete="email" />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Phone Number (for login)</IonLabel>
                        <IonInput type="tel" value={adminPhone} onIonInput={e => setAdminPhone(e.detail.value ?? '')} placeholder="+1 234 567 8900" />
                        <IonNote slot="helper">You can use this to log in instead of email</IonNote>
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Password *</IonLabel>
                        <IonInput type="password" value={password} onIonInput={e => setPassword(e.detail.value ?? '')} />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Confirm Password *</IonLabel>
                        <IonInput
                            type="password"
                            value={confirmPassword}
                            onIonInput={e => setConfirmPassword(e.detail.value ?? '')}
                            onKeyUp={e => { if (e.key === 'Enter') handleSubmit(); }}
                        />
                    </IonItem>

                    {error && (
                        <IonText color="danger">
                            <p style={{ marginTop: 8, paddingLeft: 16, fontSize: 14 }}>{error}</p>
                        </IonText>
                    )}

                    <IonButton expand="block" style={{ marginTop: 28 }} onClick={handleSubmit} disabled={registerLab.isPending}>
                        {registerLab.isPending ? <IonSpinner name="crescent" /> : 'Create Lab & Sign In'}
                    </IonButton>

                    <div style={{ textAlign: 'center', marginTop: 16 }}>
                        <IonButton fill="clear" size="small" routerLink="/login">
                            Already have an account? Sign in
                        </IonButton>
                    </div>
                </div>
            </IonContent>
        </IonPage>
    );
};

export default RegisterLabPage;
