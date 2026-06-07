import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonButton, IonSpinner, IonText, IonIcon, IonButtons, IonBackButton,
    useIonToast,
} from '@ionic/react';
import { checkmarkCircleOutline } from 'ionicons/icons';
import { useChangePassword } from '../hooks/useApi';
import { useIonRouter } from '@ionic/react';

const Field: React.FC<{ label: string; children: React.ReactNode }> = ({ label, children }) => (
    <div style={{ marginBottom: 16 }}>
        <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 4 }}>
            {label}
        </label>
        {children}
    </div>
);

const inputStyle: React.CSSProperties = {
    width: '100%',
    padding: '10px 12px',
    fontSize: 16,
    border: '1px solid var(--ion-color-light-shade)',
    borderRadius: 8,
    background: 'var(--ion-color-light)',
    color: 'var(--ion-color-dark)',
    boxSizing: 'border-box',
    outline: 'none',
};

const ChangePasswordPage: React.FC = () => {
    const router = useIonRouter();
    const [presentToast] = useIonToast();
    const changePassword = useChangePassword();

    const [current, setCurrent] = useState('');
    const [next, setNext] = useState('');
    const [confirm, setConfirm] = useState('');
    const [error, setError] = useState('');
    const [done, setDone] = useState(false);

    const handleSubmit = async () => {
        setError('');
        if (!current) { setError('Enter your current password.'); return; }
        if (!next || next.length < 8) { setError('New password must be at least 8 characters.'); return; }
        if (next !== confirm) { setError('Passwords do not match.'); return; }
        if (current === next) { setError('New password must differ from current.'); return; }

        try {
            await changePassword.mutateAsync({ currentPassword: current, newPassword: next });
            setDone(true);
            presentToast({ message: 'Password changed!', duration: 2000, color: 'success' });
        } catch (err: unknown) {
            const axErr = err as { response?: { data?: { message?: string } } };
            setError(axErr.response?.data?.message ?? 'Failed to change password.');
        }
    };

    if (done) {
        return (
            <IonPage>
                <IonHeader>
                    <IonToolbar color="primary">
                        <IonButtons slot="start"><IonBackButton defaultHref="/more" /></IonButtons>
                        <IonTitle>Change Password</IonTitle>
                    </IonToolbar>
                </IonHeader>
                <IonContent className="ion-padding">
                    <div style={{ textAlign: 'center', padding: '48px 24px' }}>
                        <IonIcon icon={checkmarkCircleOutline} style={{ fontSize: 64, color: 'var(--ion-color-success)' }} />
                        <h2>Password Updated!</h2>
                        <p style={{ color: 'var(--ion-color-medium)' }}>Your password has been changed.</p>
                        <IonButton expand="block" style={{ marginTop: 24 }} onClick={() => router.push('/more')}>
                            Done
                        </IonButton>
                    </div>
                </IonContent>
            </IonPage>
        );
    }

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar color="primary">
                    <IonButtons slot="start"><IonBackButton defaultHref="/more" /></IonButtons>
                    <IonTitle>Change Password</IonTitle>
                </IonToolbar>
            </IonHeader>
            <IonContent className="ion-padding">
                <Field label="CURRENT PASSWORD">
                    <input
                        style={inputStyle}
                        type="password"
                        placeholder="Your current password"
                        value={current}
                        onChange={e => setCurrent(e.target.value)}
                    />
                </Field>
                <Field label="NEW PASSWORD">
                    <input
                        style={inputStyle}
                        type="password"
                        placeholder="Min. 8 characters"
                        value={next}
                        onChange={e => setNext(e.target.value)}
                    />
                </Field>
                <Field label="CONFIRM NEW PASSWORD">
                    <input
                        style={inputStyle}
                        type="password"
                        placeholder="Repeat new password"
                        value={confirm}
                        onChange={e => setConfirm(e.target.value)}
                    />
                </Field>

                {error && <IonText color="danger"><p style={{ fontSize: 13 }}>{error}</p></IonText>}

                <IonButton
                    expand="block"
                    style={{ marginTop: 8 }}
                    onClick={handleSubmit}
                    disabled={changePassword.isPending}
                >
                    {changePassword.isPending ? <IonSpinner name="crescent" /> : 'Change Password'}
                </IonButton>
            </IonContent>
        </IonPage>
    );
};

export default ChangePasswordPage;
