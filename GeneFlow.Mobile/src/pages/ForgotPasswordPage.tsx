import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonButton, IonSpinner, IonText, IonIcon, IonButtons, IonBackButton,
    useIonRouter,
} from '@ionic/react';
import { checkmarkCircleOutline } from 'ionicons/icons';
import { useForgotPassword, useResetPassword } from '../hooks/useApi';

const normalizePhone = (v: string) => v.replace(/\D/g, '');

const Field: React.FC<{
    label: string;
    children: React.ReactNode;
}> = ({ label, children }) => (
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

const ForgotPasswordPage: React.FC = () => {
    const router = useIonRouter();
    const forgotPassword = useForgotPassword();
    const resetPassword = useResetPassword();

    const [step, setStep] = useState<'request' | 'reset' | 'done'>('request');
    const [mode, setMode] = useState<'phone' | 'email'>('phone');

    // Step 1 fields
    const [phone, setPhone] = useState('');
    const [email, setEmail] = useState('');
    const [resetCode, setResetCode] = useState('');

    // Step 2 fields
    const [enteredCode, setEnteredCode] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const [error, setError] = useState('');

    const handleRequestCode = async () => {
        setError('');
        const payload = mode === 'phone'
            ? { phoneNumber: normalizePhone(phone) }
            : { email: email.trim() };

        if (mode === 'phone' && !normalizePhone(phone)) {
            setError('Enter your phone number.'); return;
        }
        if (mode === 'email' && !email.trim()) {
            setError('Enter your email address.'); return;
        }

        try {
            const result = await forgotPassword.mutateAsync(payload);
            if (result?.resetCode) {
                setResetCode(result.resetCode);
            }
            setStep('reset');
        } catch {
            setError('Something went wrong. Please try again.');
        }
    };

    const handleResetPassword = async () => {
        setError('');
        if (!enteredCode.trim()) { setError('Enter the reset code.'); return; }
        if (!newPassword || newPassword.length < 8) { setError('Password must be at least 8 characters.'); return; }
        if (newPassword !== confirmPassword) { setError('Passwords do not match.'); return; }

        const payload = mode === 'phone'
            ? { phoneNumber: normalizePhone(phone), resetCode: enteredCode.trim(), newPassword }
            : { email: email.trim(), resetCode: enteredCode.trim(), newPassword };

        try {
            await resetPassword.mutateAsync(payload);
            setStep('done');
        } catch (err: unknown) {
            const axErr = err as { response?: { data?: { message?: string } } };
            setError(axErr.response?.data?.message ?? 'Invalid or expired code.');
        }
    };

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar color="primary">
                    <IonButtons slot="start">
                        <IonBackButton defaultHref="/login" />
                    </IonButtons>
                    <IonTitle>Forgot Password</IonTitle>
                </IonToolbar>
            </IonHeader>

            <IonContent className="ion-padding">

                {step === 'request' && (
                    <>
                        <div style={{ marginBottom: 24 }}>
                            <IonText color="medium">
                                <p style={{ margin: 0 }}>Enter the phone number or email you used when registering.</p>
                            </IonText>
                        </div>

                        {/* Mode toggle */}
                        <div style={{ display: 'flex', gap: 8, marginBottom: 20 }}>
                            {(['phone', 'email'] as const).map(m => (
                                <button
                                    key={m}
                                    onClick={() => { setMode(m); setError(''); }}
                                    style={{
                                        flex: 1, padding: '8px 0', borderRadius: 8,
                                        border: 'none', cursor: 'pointer', fontWeight: 600, fontSize: 14,
                                        background: mode === m ? 'var(--ion-color-primary)' : 'var(--ion-color-light)',
                                        color: mode === m ? 'white' : 'var(--ion-color-dark)',
                                    }}
                                >{m === 'phone' ? 'Phone' : 'Email'}</button>
                            ))}
                        </div>

                        {mode === 'phone' ? (
                            <Field label="PHONE NUMBER (digits only)">
                                <input
                                    style={inputStyle}
                                    type="tel"
                                    inputMode="numeric"
                                    placeholder="e.g. 7323317354"
                                    value={phone}
                                    onChange={e => setPhone(e.target.value)}
                                />
                            </Field>
                        ) : (
                            <Field label="EMAIL ADDRESS">
                                <input
                                    style={inputStyle}
                                    type="email"
                                    placeholder="you@example.com"
                                    value={email}
                                    onChange={e => setEmail(e.target.value)}
                                />
                            </Field>
                        )}

                        {error && <IonText color="danger"><p style={{ fontSize: 13 }}>{error}</p></IonText>}

                        <IonButton
                            expand="block"
                            style={{ marginTop: 8 }}
                            onClick={handleRequestCode}
                            disabled={forgotPassword.isPending}
                        >
                            {forgotPassword.isPending ? <IonSpinner name="crescent" /> : 'Get Reset Code'}
                        </IonButton>

                        <div style={{ textAlign: 'center', marginTop: 16 }}>
                            <IonButton fill="clear" size="small" onClick={() => router.push('/login')}>
                                Back to Login
                            </IonButton>
                        </div>
                    </>
                )}

                {step === 'reset' && (
                    <>
                        <div style={{ marginBottom: 24 }}>
                            <IonText color="medium">
                                <p style={{ margin: 0 }}>
                                    Your reset code is shown below (in production this would be sent via SMS).
                                    Enter it along with your new password.
                                </p>
                            </IonText>
                        </div>

                        {resetCode && (
                            <div style={{
                                background: 'var(--ion-color-warning-tint)',
                                border: '1px solid var(--ion-color-warning)',
                                borderRadius: 8, padding: '12px 16px', marginBottom: 20,
                                textAlign: 'center'
                            }}>
                                <div style={{ fontSize: 11, color: 'var(--ion-color-medium)', marginBottom: 4 }}>YOUR RESET CODE</div>
                                <div style={{ fontSize: 28, fontWeight: 700, letterSpacing: 8, color: 'var(--ion-color-dark)' }}>
                                    {resetCode}
                                </div>
                                <div style={{ fontSize: 11, color: 'var(--ion-color-medium)', marginTop: 4 }}>Valid for 15 minutes</div>
                            </div>
                        )}

                        <Field label="RESET CODE">
                            <input
                                style={inputStyle}
                                type="text"
                                inputMode="numeric"
                                placeholder="6-digit code"
                                value={enteredCode}
                                onChange={e => setEnteredCode(e.target.value)}
                            />
                        </Field>

                        <Field label="NEW PASSWORD">
                            <input
                                style={inputStyle}
                                type="password"
                                placeholder="Min. 8 characters"
                                value={newPassword}
                                onChange={e => setNewPassword(e.target.value)}
                            />
                        </Field>

                        <Field label="CONFIRM NEW PASSWORD">
                            <input
                                style={inputStyle}
                                type="password"
                                placeholder="Repeat new password"
                                value={confirmPassword}
                                onChange={e => setConfirmPassword(e.target.value)}
                            />
                        </Field>

                        {error && <IonText color="danger"><p style={{ fontSize: 13 }}>{error}</p></IonText>}

                        <IonButton
                            expand="block"
                            style={{ marginTop: 8 }}
                            onClick={handleResetPassword}
                            disabled={resetPassword.isPending}
                        >
                            {resetPassword.isPending ? <IonSpinner name="crescent" /> : 'Reset Password'}
                        </IonButton>

                        <div style={{ textAlign: 'center', marginTop: 8 }}>
                            <IonButton fill="clear" size="small" onClick={() => setStep('request')}>
                                ← Back
                            </IonButton>
                        </div>
                    </>
                )}

                {step === 'done' && (
                    <div style={{ textAlign: 'center', padding: '48px 24px' }}>
                        <IonIcon icon={checkmarkCircleOutline} style={{ fontSize: 64, color: 'var(--ion-color-success)' }} />
                        <h2 style={{ marginTop: 16 }}>Password Reset!</h2>
                        <p style={{ color: 'var(--ion-color-medium)' }}>Your password has been updated. Please log in.</p>
                        <IonButton expand="block" style={{ marginTop: 24 }} onClick={() => router.push('/login')}>
                            Go to Login
                        </IonButton>
                    </div>
                )}
            </IonContent>
        </IonPage>
    );
};

export default ForgotPasswordPage;
