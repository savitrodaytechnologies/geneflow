import React, { useState } from 'react';
import {
    IonPage, IonHeader, IonToolbar, IonTitle, IonContent,
    IonList, IonItem, IonLabel, IonBadge, IonButton, IonButtons,
    IonFab, IonFabButton, IonIcon, IonModal, IonInput, IonSelect,
    IonSelectOption, IonAlert, IonSpinner, IonText, IonNote,
    IonAvatar, IonChip, IonRefresher, IonRefresherContent,
} from '@ionic/react';
import { add, personRemove } from 'ionicons/icons';
import { useAuth } from '../context/AuthContext';
import { useLabMembers, useAddLabMember, useRemoveLabMember } from '../hooks/useApi';
import type { AddLabUserRequest } from '../types/api';

const ROLE_COLOR: Record<string, string> = {
    LabAdmin: 'primary',
    Researcher: 'secondary',
    Viewer: 'medium',
};

const TeamPage: React.FC = () => {
    const { user } = useAuth();
    const labId = user?.labId ?? '';
    const isAdmin = user?.labRole === 'LabAdmin';

    const { data: members, isLoading, refetch } = useLabMembers(labId);
    const addMember = useAddLabMember(labId);
    const removeMember = useRemoveLabMember(labId);

    const [showAddModal, setShowAddModal] = useState(false);
    const [removeTarget, setRemoveTarget] = useState<{ userId: string; name: string } | null>(null);

    // Add member form state
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
                                    <IonButton
                                        fill="clear"
                                        color="danger"
                                        slot="end"
                                        onClick={() => setRemoveTarget({ userId: m.userId, name: m.fullName })}
                                    >
                                        <IonIcon icon={personRemove} slot="icon-only" />
                                    </IonButton>
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
                    <IonItem>
                        <IonLabel position="floating">Full Name *</IonLabel>
                        <IonInput value={form.fullName} onIonInput={e => setForm(f => ({ ...f, fullName: e.detail.value ?? '' }))} />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Phone Number</IonLabel>
                        <IonInput type="tel" value={form.phoneNumber} onIonInput={e => setForm(f => ({ ...f, phoneNumber: e.detail.value ?? '' }))} />
                        <IonNote slot="helper">Preferred — used for login</IonNote>
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Email</IonLabel>
                        <IonInput type="email" value={form.email} onIonInput={e => setForm(f => ({ ...f, email: e.detail.value ?? '' }))} />
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel position="floating">Temporary Password *</IonLabel>
                        <IonInput type="password" value={form.password} onIonInput={e => setForm(f => ({ ...f, password: e.detail.value ?? '' }))} />
                        <IonNote slot="helper">Share this with the new member so they can log in</IonNote>
                    </IonItem>
                    <IonItem style={{ marginTop: 8 }}>
                        <IonLabel>Role</IonLabel>
                        <IonSelect value={form.labRole} onIonChange={e => setForm(f => ({ ...f, labRole: e.detail.value }))}>
                            <IonSelectOption value="Researcher">Researcher</IonSelectOption>
                            <IonSelectOption value="LabAdmin">Lab Admin</IonSelectOption>
                            <IonSelectOption value="Viewer">Viewer</IonSelectOption>
                        </IonSelect>
                    </IonItem>
                    {addError && (
                        <IonText color="danger">
                            <p style={{ paddingLeft: 16, fontSize: 14 }}>{addError}</p>
                        </IonText>
                    )}
                    <IonButton expand="block" style={{ marginTop: 24 }} onClick={handleAdd} disabled={addMember.isPending}>
                        {addMember.isPending ? <IonSpinner name="crescent" /> : 'Add Member'}
                    </IonButton>
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
        </IonPage>
    );
};

export default TeamPage;
