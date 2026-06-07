import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonList, IonItem, IonLabel, IonButton, IonButtons,
    IonIcon, IonModal, IonSelect, IonSelectOption,
    IonAlert, IonSpinner, IonText,
    IonAvatar, IonChip, IonRefresher, IonRefresherContent,
    useIonToast,
} from '@ionic/react';
import { add, personRemove, keyOutline } from 'ionicons/icons';
import { useAuth } from '../context/AuthContext';
import { useLabMembers, useAddLabMember, useRemoveLabMember, useAdminResetPassword } from '../hooks/useApi';
import type { AddLabUserRequest } from '../types/api';

const ROLE_COLOR: Record<string, string> = {
    LabAdmin: 'primary',
    Researcher: 'secondary',
    Viewer: 'medium',
};

// Shared field for modal — no floating label overlap
const MField: React.FC<{
    label: string; type?: string; value: string;
    onChange: (v: string) => void; placeholder?: string; hint?: string;
}> = ({ label, type = 'text', value, onChange, placeholder, hint }) => (
    <div style={{ marginBottom: 14 }}>
        <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 5, paddingLeft: 2 }}>
            {label}
        </label>
        <input
            type={type}
            value={value}
            onChange={e => onChange(e.target.value)}
            placeholder={placeholder}
            inputMode={type === 'tel' ? 'numeric' : undefined}
            style={{
                width: '100%', boxSizing: 'border-box',
                padding: '11px 14px', fontSize: 16,
                borderRadius: 10, border: '1.5px solid var(--ion-color-light-shade)',
                background: 'var(--ion-item-background, var(--ion-background-color))',
                color: 'var(--ion-text-color)', outline: 'none',
            }}
        />
        {hint && <p style={{ fontSize: 12, color: 'var(--ion-color-medium)', margin: '3px 0 0 2px' }}>{hint}</p>}
    </div>
);

const TeamPage: React.FC = () => {
    const { user } = useAuth();
    const labId = user?.labId ?? '';
    const isAdmin = user?.labRole === 'LabAdmin';
    const [presentToast] = useIonToast();

    const { data: members, isLoading, refetch } = useLabMembers(labId);
    const addMember = useAddLabMember(labId);
    const removeMember = useRemoveLabMember(labId);
    const resetPw = useAdminResetPassword(labId);

    const [showAddModal, setShowAddModal] = useState(false);
    const [removeTarget, setRemoveTarget] = useState<{ userId: string; name: string } | null>(null);
    const [resetTarget, setResetTarget] = useState<{ userId: string; name: string } | null>(null);
    const [resetPwValue, setResetPwValue] = useState('');
    const [resetResult, setResetResult] = useState('');
    const [showResetModal, setShowResetModal] = useState(false);

    const [form, setForm] = useState<AddLabUserRequest>({
        fullName: '', email: '', phoneNumber: '', password: '', labRole: 'Researcher',
    });
    const [addError, setAddError] = useState('');

    const resetForm = () => {
        setForm({ fullName: '', email: '', phoneNumber: '', password: '', labRole: 'Researcher' });
        setAddError('');
    };

    const handleAdd = async () => {
        setAddError('');
        if (!form.fullName.trim()) { setAddError('Full name is required.'); return; }
        if (!form.email?.trim() && !form.phoneNumber?.trim()) { setAddError('Email or phone required.'); return; }
        if (!form.password || form.password.length < 8) { setAddError('Password must be at least 8 characters.'); return; }
        try {
            await addMember.mutateAsync(form);
            setShowAddModal(false);
            resetForm();
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setAddError(axiosErr.response?.data?.message ?? 'Failed to add member.');
        }
    };

    const handleRemove = async () => {
        if (!removeTarget) return;
        await removeMember.mutateAsync(removeTarget.userId);
        setRemoveTarget(null);
    };

    const handleAdminReset = async () => {
        if (!resetTarget) return;
        try {
            const result = await resetPw.mutateAsync({ userId: resetTarget.userId, newPassword: resetPwValue });
            const tempPw = (result as any).temporaryPassword;
            setResetResult(tempPw ?? 'Password reset successfully.');
            setResetPwValue('');
        } catch (err: unknown) {
            const axErr = err as { response?: { data?: { message?: string } } };
            presentToast({ message: axErr.response?.data?.message ?? 'Reset failed.', duration: 3000, color: 'danger' });
            setShowResetModal(false);
            setResetTarget(null);
        }
    };

    const initials = (name: string) => name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);

    return (
        <IonPage>
            <IonHeader>
                <IonToolbar color="primary">
                    <IonTitle>Team</IonTitle>
                    {isAdmin && (
                        <IonButtons slot="end">
                            <IonButton onClick={() => { resetForm(); setShowAddModal(true); }}>
                                <IonIcon icon={add} slot="icon-only" />
                            </IonButton>
                        </IonButtons>
                    )}
                </IonToolbar>
            </IonHeader>

            <IonContent>
                <IonRefresher slot="fixed" onIonRefresh={async e => { await refetch(); e.detail.complete(); }}>
                    <IonRefresherContent />
                </IonRefresher>

                {isLoading ? (
                    <div style={{ display: 'flex', justifyContent: 'center', marginTop: 48 }}>
                        <IonSpinner />
                    </div>
                ) : (
                    <IonList>
                        {(members ?? []).map(m => (
                            <IonItem key={m.userId}>
                                <IonAvatar slot="start" style={{ background: 'var(--ion-color-primary)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                                    <span style={{ color: 'white', fontWeight: 600, fontSize: 14 }}>{initials(m.fullName)}</span>
                                </IonAvatar>
                                <IonLabel>
                                    <h2>{m.fullName}</h2>
                                    <p>{m.phoneNumber || m.email}</p>
                                    {m.phoneNumber && m.email && <p style={{ fontSize: 12 }}>{m.email}</p>}
                                </IonLabel>
                                <IonChip color={ROLE_COLOR[m.labRole] ?? 'medium'} slot="end">
                                    {m.labRole}
                                </IonChip>
                                {isAdmin && m.userId !== user?.userId && (
                                    <>
                                        <IonButton fill="clear" color="medium" slot="end"
                                            onClick={() => { setResetTarget({ userId: m.userId, name: m.fullName }); setResetPwValue(''); setResetResult(''); setShowResetModal(true); }}>
                                            <IonIcon icon={keyOutline} slot="icon-only" />
                                        </IonButton>
                                        <IonButton fill="clear" color="danger" slot="end"
                                            onClick={() => setRemoveTarget({ userId: m.userId, name: m.fullName })}>
                                            <IonIcon icon={personRemove} slot="icon-only" />
                                        </IonButton>
                                    </>
                                )}
                            </IonItem>
                        ))}
                        {(members ?? []).length === 0 && (
                            <div style={{ textAlign: 'center', padding: 40 }}>
                                <IonText color="medium">No team members yet.</IonText>
                            </div>
                        )}
                    </IonList>
                )}
            </IonContent>

            {/* Add Member Modal */}
            <IonModal isOpen={showAddModal} onDidDismiss={() => setShowAddModal(false)}>
                <IonHeader>
                    <IonToolbar>
                        <IonTitle>Add Team Member</IonTitle>
                        <IonButtons slot="end">
                            <IonButton onClick={() => setShowAddModal(false)}>Cancel</IonButton>
                        </IonButtons>
                    </IonToolbar>
                </IonHeader>
                <IonContent className="ion-padding">
                    <div style={{ marginTop: 8 }}>
                        <MField label="Full Name *" value={form.fullName} onChange={v => setForm(f => ({ ...f, fullName: v }))} placeholder="Dr. Jane Smith" />
                        <MField label="Phone Number (preferred for login)" type="tel" value={form.phoneNumber ?? ''}
                            onChange={v => setForm(f => ({ ...f, phoneNumber: v }))}
                            placeholder="9876543210 (digits only)" hint="Include country code if needed, e.g. 19876543210" />
                        <MField label="Email" type="email" value={form.email ?? ''} onChange={v => setForm(f => ({ ...f, email: v }))} placeholder="user@lab.com" />
                        <MField label="Temporary Password *" type="password" value={form.password} onChange={v => setForm(f => ({ ...f, password: v }))}
                            hint="Share this with the new member so they can log in" />

                        <div style={{ marginBottom: 14 }}>
                            <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 5, paddingLeft: 2 }}>
                                Role
                            </label>
                            <IonItem lines="none" style={{ borderRadius: 10, border: '1.5px solid var(--ion-color-light-shade)' }}>
                                <IonSelect value={form.labRole} onIonChange={e => setForm(f => ({ ...f, labRole: e.detail.value }))}>
                                    <IonSelectOption value="Researcher">Researcher</IonSelectOption>
                                    <IonSelectOption value="LabAdmin">Lab Admin</IonSelectOption>
                                    <IonSelectOption value="Viewer">Viewer</IonSelectOption>
                                </IonSelect>
                            </IonItem>
                        </div>

                        {addError && (
                            <IonText color="danger">
                                <p style={{ fontSize: 13, marginBottom: 8 }}>{addError}</p>
                            </IonText>
                        )}
                        <IonButton expand="block" style={{ marginTop: 8 }} onClick={handleAdd} disabled={addMember.isPending}>
                            {addMember.isPending ? <IonSpinner name="crescent" /> : 'Add Member'}
                        </IonButton>
                    </div>
                </IonContent>
            </IonModal>

            {/* Remove Confirm Alert */}
            <IonAlert
                isOpen={!!removeTarget}
                header="Remove Member"
                message={`Remove ${removeTarget?.name} from the lab?`}
                buttons={[
                    { text: 'Cancel', role: 'cancel', handler: () => setRemoveTarget(null) },
                    { text: 'Remove', role: 'destructive', handler: handleRemove },
                ]}
                onDidDismiss={() => setRemoveTarget(null)}
            />

            {/* Admin Reset Password Modal */}
            <IonModal isOpen={showResetModal} onDidDismiss={() => { setShowResetModal(false); setResetTarget(null); setResetResult(''); }} breakpoints={[0, 0.6]} initialBreakpoint={0.6}>
                <IonHeader>
                    <IonToolbar>
                        <IonTitle>Reset Password</IonTitle>
                        <IonButtons slot="end">
                            <IonButton onClick={() => { setShowResetModal(false); setResetTarget(null); setResetResult(''); }}>Done</IonButton>
                        </IonButtons>
                    </IonToolbar>
                </IonHeader>
                <IonContent className="ion-padding">
                    {!resetResult ? (
                        <>
                            <IonText color="medium">
                                <p style={{ marginTop: 0 }}>Set a new temporary password for <strong>{resetTarget?.name}</strong>. Leave blank to auto-generate.</p>
                            </IonText>
                            <div style={{ marginBottom: 16 }}>
                                <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--ion-color-medium)', marginBottom: 5 }}>
                                    NEW PASSWORD (optional)
                                </label>
                                <input
                                    type="password"
                                    value={resetPwValue}
                                    onChange={e => setResetPwValue(e.target.value)}
                                    placeholder="Leave blank to auto-generate"
                                    style={{ width: '100%', boxSizing: 'border-box', padding: '11px 14px', fontSize: 16, borderRadius: 10, border: '1.5px solid var(--ion-color-light-shade)', background: 'var(--ion-item-background, var(--ion-background-color))', color: 'var(--ion-text-color)', outline: 'none' }}
                                />
                            </div>
                            <IonButton expand="block" onClick={handleAdminReset} disabled={resetPw.isPending}>
                                {resetPw.isPending ? <IonSpinner name="crescent" /> : 'Reset Password'}
                            </IonButton>
                        </>
                    ) : (
                        <div style={{ textAlign: 'center', padding: '16px 0' }}>
                            <IonText color="success"><p style={{ fontSize: 16, fontWeight: 600 }}>Password Reset!</p></IonText>
                            <IonText color="medium"><p>Share this temporary password with {resetTarget?.name}:</p></IonText>
                            <div style={{ background: 'var(--ion-color-light)', borderRadius: 8, padding: '16px', fontSize: 22, fontWeight: 700, letterSpacing: 2, color: 'var(--ion-color-dark)', margin: '16px 0', userSelect: 'all' }}>
                                {resetResult}
                            </div>
                            <IonText color="medium"><p style={{ fontSize: 12 }}>Ask them to change it after logging in.</p></IonText>
                        </div>
                    )}
                </IonContent>
            </IonModal>
        </IonPage>
    );
};

export default TeamPage;
