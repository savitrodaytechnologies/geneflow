import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent, IonButtons,
    IonBackButton, IonButton, IonText, IonSpinner, IonSelect, IonSelectOption,
} from '@ionic/react';
import { useHistory } from 'react-router-dom';
import { useRegisterLab } from '../hooks/useApi';
import { useAuth } from '../context/AuthContext';

// ── Common country dial codes ────────────────────────────────────────────────
const DIAL_CODES = [
    { code: '+1', flag: '🇺🇸', name: 'USA / Canada' },
    { code: '+44', flag: '🇬🇧', name: 'UK' },
    { code: '+91', flag: '🇮🇳', name: 'India' },
    { code: '+61', flag: '🇦🇺', name: 'Australia' },
    { code: '+49', flag: '🇩🇪', name: 'Germany' },
    { code: '+33', flag: '🇫🇷', name: 'France' },
    { code: '+81', flag: '🇯🇵', name: 'Japan' },
    { code: '+86', flag: '🇨🇳', name: 'China' },
    { code: '+82', flag: '🇰🇷', name: 'South Korea' },
    { code: '+55', flag: '🇧🇷', name: 'Brazil' },
    { code: '+52', flag: '🇲🇽', name: 'Mexico' },
    { code: '+34', flag: '🇪🇸', name: 'Spain' },
    { code: '+39', flag: '🇮🇹', name: 'Italy' },
    { code: '+7', flag: '🇷🇺', name: 'Russia' },
    { code: '+27', flag: '🇿🇦', name: 'South Africa' },
    { code: '+20', flag: '🇪🇬', name: 'Egypt' },
    { code: '+234', flag: '🇳🇬', name: 'Nigeria' },
    { code: '+254', flag: '🇰🇪', name: 'Kenya' },
    { code: '+65', flag: '🇸🇬', name: 'Singapore' },
    { code: '+60', flag: '🇲🇾', name: 'Malaysia' },
    { code: '+66', flag: '🇹🇭', name: 'Thailand' },
    { code: '+92', flag: '🇵🇰', name: 'Pakistan' },
    { code: '+880', flag: '🇧🇩', name: 'Bangladesh' },
    { code: '+94', flag: '🇱🇰', name: 'Sri Lanka' },
    { code: '+971', flag: '🇦🇪', name: 'UAE' },
    { code: '+966', flag: '🇸🇦', name: 'Saudi Arabia' },
    { code: '+972', flag: '🇮🇱', name: 'Israel' },
    { code: '+90', flag: '🇹🇷', name: 'Turkey' },
    { code: '+31', flag: '🇳🇱', name: 'Netherlands' },
    { code: '+46', flag: '🇸🇪', name: 'Sweden' },
    { code: '+47', flag: '🇳🇴', name: 'Norway' },
    { code: '+45', flag: '🇩🇰', name: 'Denmark' },
    { code: '+358', flag: '🇫🇮', name: 'Finland' },
    { code: '+41', flag: '🇨🇭', name: 'Switzerland' },
    { code: '+43', flag: '🇦🇹', name: 'Austria' },
    { code: '+32', flag: '🇧🇪', name: 'Belgium' },
    { code: '+48', flag: '🇵🇱', name: 'Poland' },
    { code: '+64', flag: '🇳🇿', name: 'New Zealand' },
    { code: '+1', flag: '🇨🇦', name: 'Canada' },
    { code: '+54', flag: '🇦🇷', name: 'Argentina' },
];

// ── Shared Field component ───────────────────────────────────────────────────
const Field: React.FC<{
    label: string; type?: string; value: string;
    onChange: (v: string) => void; placeholder?: string;
    onEnter?: () => void; inputMode?: string; autocomplete?: string;
    hint?: string; required?: boolean;
}> = ({ label, type = 'text', value, onChange, placeholder, onEnter, inputMode, autocomplete, hint, required }) => (
    <div style={{ marginBottom: 14 }}>
        <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 5, paddingLeft: 2 }}>
            {label}{required && <span style={{ color: 'var(--ion-color-danger)' }}> *</span>}
        </label>
        <input
            type={type}
            value={value}
            onChange={e => onChange(e.target.value)}
            onKeyUp={e => { if (e.key === 'Enter' && onEnter) onEnter(); }}
            placeholder={placeholder}
            inputMode={inputMode as any}
            autoComplete={autocomplete}
            style={{
                width: '100%', boxSizing: 'border-box',
                padding: '11px 14px', fontSize: 16,
                borderRadius: 10, border: '1.5px solid var(--ion-color-light-shade)',
                background: 'var(--ion-item-background, var(--ion-background-color))',
                color: 'var(--ion-text-color)', outline: 'none',
            }}
        />
        {hint && <p style={{ fontSize: 12, color: 'var(--ion-color-medium)', margin: '4px 0 0 2px' }}>{hint}</p>}
    </div>
);

// ── Section heading ──────────────────────────────────────────────────────────
const Section: React.FC<{ title: string }> = ({ title }) => (
    <p style={{ fontSize: 12, fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase', color: 'var(--ion-color-medium)', margin: '20px 0 10px 2px' }}>
        {title}
    </p>
);

const RegisterLabPage: React.FC = () => {
    const [labName, setLabName] = useState('');
    const [institution, setInstitution] = useState('');
    const [adminName, setAdminName] = useState('');
    const [adminEmail, setAdminEmail] = useState('');
    const [dialCode, setDialCode] = useState('+1');
    const [phoneLocal, setPhoneLocal] = useState('');
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

        // Combine dial code + local number (digits only)
        const digits = phoneLocal.replace(/\D/g, '');
        const fullPhone = digits ? `${dialCode}${digits}` : undefined;

        try {
            const result = await registerLab.mutateAsync({
                labName: labName.trim(),
                institutionName: institution.trim() || undefined,
                adminFullName: adminName.trim(),
                adminEmail: adminEmail.trim(),
                adminPhoneNumber: fullPhone,
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
                <div style={{ maxWidth: 480, margin: '8px auto' }}>
                    <p style={{ color: 'var(--ion-color-medium)', fontSize: 14, marginBottom: 4 }}>
                        Create your lab account. You'll be the Lab Admin and can add team members after.
                    </p>

                    <Section title="Lab Details" />
                    <Field label="Lab Name" value={labName} onChange={setLabName} required placeholder="e.g. Smith Genomics Lab" />
                    <Field label="Institution / University" value={institution} onChange={setInstitution} placeholder="e.g. MIT" />

                    <Section title="Your Account" />
                    <Field label="Full Name" value={adminName} onChange={setAdminName} required placeholder="Dr. Jane Smith" autocomplete="name" />
                    <Field label="Email" type="email" value={adminEmail} onChange={setAdminEmail} required placeholder="you@university.edu" autocomplete="email" inputMode="email" />

                    {/* Phone: country code picker + local number */}
                    <div style={{ marginBottom: 14 }}>
                        <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 5, paddingLeft: 2 }}>
                            Phone Number <span style={{ fontSize: 11, fontWeight: 400 }}>(optional — used for login)</span>
                        </label>
                        <div style={{ display: 'flex', gap: 8 }}>
                            <div style={{
                                flexShrink: 0, border: '1.5px solid var(--ion-color-light-shade)',
                                borderRadius: 10, overflow: 'hidden', background: 'var(--ion-item-background, var(--ion-background-color))',
                            }}>
                                <IonSelect
                                    value={dialCode}
                                    onIonChange={e => setDialCode(e.detail.value)}
                                    interface="action-sheet"
                                    style={{ minWidth: 90, padding: '10px 4px' }}
                                >
                                    {DIAL_CODES.map(c => (
                                        <IonSelectOption key={`${c.code}-${c.name}`} value={c.code}>
                                            {c.flag} {c.code}
                                        </IonSelectOption>
                                    ))}
                                </IonSelect>
                            </div>
                            <input
                                type="tel"
                                value={phoneLocal}
                                onChange={e => setPhoneLocal(e.target.value)}
                                placeholder="9876543210"
                                inputMode="numeric"
                                autoComplete="tel-national"
                                style={{
                                    flex: 1, padding: '11px 14px', fontSize: 16,
                                    borderRadius: 10, border: '1.5px solid var(--ion-color-light-shade)',
                                    background: 'var(--ion-item-background, var(--ion-background-color))',
                                    color: 'var(--ion-text-color)', outline: 'none', boxSizing: 'border-box',
                                }}
                            />
                        </div>
                        <p style={{ fontSize: 12, color: 'var(--ion-color-medium)', margin: '4px 0 0 2px' }}>
                            Select your country, then enter your number without the country code
                        </p>
                    </div>

                    <Field label="Password" type="password" value={password} onChange={setPassword} required autocomplete="new-password"
                        hint="At least 8 characters" />
                    <Field label="Confirm Password" type="password" value={confirmPassword} onChange={setConfirmPassword} required
                        autocomplete="new-password" onEnter={handleSubmit} />

                    {error && (
                        <IonText color="danger">
                            <p style={{ marginTop: 4, marginBottom: 8, fontSize: 13 }}>{error}</p>
                        </IonText>
                    )}

                    <IonButton expand="block" style={{ marginTop: 20 }} onClick={handleSubmit} disabled={registerLab.isPending}>
                        {registerLab.isPending ? <IonSpinner name="crescent" /> : 'Create Lab & Sign In'}
                    </IonButton>

                    <div style={{ textAlign: 'center', marginTop: 12 }}>
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
