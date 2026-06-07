import React, { useState } from 'react';
import {
  IonButton, IonSpinner, IonItem, IonLabel, IonInput, IonSelect,
  IonSelectOption, IonModal, IonHeader, IonToolbar, IonTitle, IonButtons,
  IonContent, IonText, IonIcon, IonBadge, IonSegment, IonSegmentButton,
  useIonToast,
} from '@ionic/react';
import { gridOutline, listOutline, flashOutline } from 'ionicons/icons';
import { usePlateGrid, useUpdateWell, useQuickFill } from '../hooks/useApi';
import type { PlateWellDto } from '../types/api';

// Well color by sample type
const WELL_COLORS: Record<string, string> = {
  sample: '#2196F3',
  control: '#4CAF50',
  NTC: '#FF9800',
  standard: '#9C27B0',
};

interface Props {
  experimentId: string;
  experiment: any;
}

const PlateSetupTab: React.FC<Props> = ({ experimentId, experiment }) => {
  const { data: plate, isLoading, refetch } = usePlateGrid(experimentId);
  const updateWell = useUpdateWell(experimentId);
  const quickFill = useQuickFill(experimentId);
  const [presentToast] = useIonToast();

  const [view, setView] = useState<'grid' | 'list'>('grid');
  const [selectedWell, setSelectedWell] = useState<PlateWellDto | null>(null);
  const [showQuickFill, setShowQuickFill] = useState(false);

  // Well edit form
  const [wellForm, setWellForm] = useState({ sampleName: '', targetGene: '', sampleType: 'sample', replicateGroup: '' });

  // Quick fill form
  const [qfForm, setQfForm] = useState({ fromWell: 'A01', toWell: 'A12', sampleName: '', targetGene: '', sampleType: 'sample', replicateGroup: '' });

  const openWell = (well: PlateWellDto) => {
    setSelectedWell(well);
    setWellForm({
      sampleName: well.sampleName ?? '',
      targetGene: well.targetGene ?? '',
      sampleType: well.sampleType ?? 'sample',
      replicateGroup: well.replicateGroup ?? '',
    });
  };

  const saveWell = async () => {
    if (!selectedWell) return;
    try {
      await updateWell.mutateAsync({ wellId: selectedWell.wellId, data: wellForm });
      setSelectedWell(null);
      presentToast({ message: `Well ${selectedWell.wellId} updated`, duration: 1500, color: 'success' });
    } catch {
      presentToast({ message: 'Failed to update well', duration: 2000, color: 'danger' });
    }
  };

  const handleQuickFill = async () => {
    try {
      const result = await quickFill.mutateAsync(qfForm);
      setShowQuickFill(false);
      presentToast({ message: `${(result as any).filledWells} wells filled`, duration: 2000, color: 'success' });
    } catch {
      presentToast({ message: 'Quick fill failed', duration: 2000, color: 'danger' });
    }
  };

  if (isLoading) {
    return <div style={{ textAlign: 'center', padding: 32 }}><IonSpinner /></div>;
  }

  if (!plate) {
    return <div style={{ textAlign: 'center', padding: 32, color: 'var(--ion-color-medium)' }}>No plate layout found.</div>;
  }

  const rows = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];

  return (
    <>
      {/* Plate stats + controls */}
      <div style={{ padding: '12px 16px 4px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <IonText color="medium" style={{ fontSize: 12 }}>
          {plate.filledWells}/{plate.totalWells} wells filled
          {plate.excludedWells > 0 && ` · ${plate.excludedWells} excluded`}
        </IonText>
        <IonButton size="small" fill="outline" onClick={() => setShowQuickFill(true)}>
          <IonIcon slot="start" icon={flashOutline} />
          Quick Fill
        </IonButton>
      </div>

      {/* View toggle */}
      <IonSegment
        value={view}
        onIonChange={e => setView(e.detail.value as 'grid' | 'list')}
        style={{ padding: '4px 16px' }}
      >
        <IonSegmentButton value="grid">
          <IonIcon icon={gridOutline} /><IonLabel>Grid</IonLabel>
        </IonSegmentButton>
        <IonSegmentButton value="list">
          <IonIcon icon={listOutline} /><IonLabel>List</IonLabel>
        </IonSegmentButton>
      </IonSegment>

      {view === 'grid' ? (
        /* 8×12 Grid View */
        <div style={{ overflowX: 'auto', padding: '8px 12px' }}>
          {/* Column headers */}
          <div style={{ display: 'flex', marginLeft: 22, marginBottom: 2 }}>
            {Array.from({ length: 12 }, (_, i) => (
              <div key={i} style={{ width: 28, textAlign: 'center', fontSize: 9, color: 'var(--ion-color-medium)', flexShrink: 0 }}>
                {i + 1}
              </div>
            ))}
          </div>
          {plate.grid.map((row, rIdx) => (
            <div key={rIdx} style={{ display: 'flex', alignItems: 'center', marginBottom: 2 }}>
              {/* Row label */}
              <div style={{ width: 18, fontSize: 10, color: 'var(--ion-color-medium)', flexShrink: 0, textAlign: 'center' }}>
                {rows[rIdx]}
              </div>
              {row.map(well => {
                const filled = !!well.sampleName;
                const bg = filled ? (WELL_COLORS[well.sampleType ?? ''] ?? '#607D8B') : 'var(--ion-color-light)';
                return (
                  <div
                    key={well.wellId}
                    onClick={() => openWell(well)}
                    style={{
                      width: 28,
                      height: 28,
                      borderRadius: '50%',
                      backgroundColor: well.isExcluded ? '#ccc' : bg,
                      border: `1px solid ${filled ? 'transparent' : 'var(--ion-color-medium)'}`,
                      cursor: 'pointer',
                      flexShrink: 0,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      opacity: well.isExcluded ? 0.4 : 1,
                    }}
                    title={`${well.wellId}: ${well.sampleName ?? 'empty'}`}
                  >
                    {filled && <div style={{ width: 6, height: 6, borderRadius: '50%', backgroundColor: 'rgba(255,255,255,0.7)' }} />}
                  </div>
                );
              })}
            </div>
          ))}
          {/* Legend */}
          <div style={{ display: 'flex', gap: 12, marginTop: 8, flexWrap: 'wrap' }}>
            {Object.entries(WELL_COLORS).map(([type, color]) => (
              <div key={type} style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 11 }}>
                <div style={{ width: 12, height: 12, borderRadius: '50%', backgroundColor: color }} />
                {type}
              </div>
            ))}
            <div style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 11 }}>
              <div style={{ width: 12, height: 12, borderRadius: '50%', backgroundColor: 'var(--ion-color-light)', border: '1px solid var(--ion-color-medium)' }} />
              empty
            </div>
          </div>
        </div>
      ) : (
        /* List View - only filled wells */
        <div style={{ padding: '4px 8px' }}>
          {plate.grid.flat().filter(w => w.sampleName).length === 0 ? (
            <div style={{ textAlign: 'center', padding: 24, color: 'var(--ion-color-medium)' }}>
              No wells filled yet. Use Quick Fill or tap a well in Grid view.
            </div>
          ) : (
            plate.grid.flat().filter(w => w.sampleName).map(well => (
              <IonItem
                key={well.wellId}
                button
                onClick={() => openWell(well)}
                lines="none"
                style={{ '--border-radius': '8px', marginBottom: 3 }}
              >
                <div
                  slot="start"
                  style={{
                    width: 32, height: 32, borderRadius: '50%',
                    backgroundColor: WELL_COLORS[well.sampleType ?? ''] ?? '#607D8B',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    color: 'white', fontSize: 10, fontWeight: 700,
                  }}
                >
                  {well.wellId}
                </div>
                <IonLabel>
                  <h3 style={{ fontSize: 13, fontWeight: 600 }}>{well.sampleName}</h3>
                  <p style={{ fontSize: 11 }}>
                    {well.targetGene ?? '—'} · {well.sampleType ?? '—'}
                    {well.replicateGroup && ` · Rep ${well.replicateGroup}`}
                  </p>
                </IonLabel>
                {well.isExcluded && <IonBadge color="medium" slot="end">Excluded</IonBadge>}
                {well.ctValue != null && <IonBadge color="tertiary" slot="end">Ct {well.ctValue}</IonBadge>}
              </IonItem>
            ))
          )}
        </div>
      )}

      {/* Well Edit Modal */}
      <IonModal
        isOpen={!!selectedWell}
        onDidDismiss={() => setSelectedWell(null)}
        breakpoints={[0, 0.75]}
        initialBreakpoint={0.75}
      >
        <IonHeader>
          <IonToolbar>
            <IonTitle>Well {selectedWell?.wellId}</IonTitle>
            <IonButtons slot="end">
              <IonButton onClick={() => setSelectedWell(null)}>Cancel</IonButton>
            </IonButtons>
          </IonToolbar>
        </IonHeader>
        <IonContent className="ion-padding">
          <IonItem>
            <IonLabel position="stacked">Sample Name</IonLabel>
            <IonInput value={wellForm.sampleName} onIonInput={e => setWellForm(f => ({ ...f, sampleName: e.detail.value ?? '' }))} placeholder="e.g. Patient_01" clearInput />
          </IonItem>
          <IonItem>
            <IonLabel position="stacked">Target Gene</IonLabel>
            <IonInput value={wellForm.targetGene} onIonInput={e => setWellForm(f => ({ ...f, targetGene: e.detail.value ?? '' }))} placeholder="e.g. CCR2, IL6" clearInput />
          </IonItem>
          <IonItem>
            <IonLabel position="stacked">Sample Type</IonLabel>
            <IonSelect value={wellForm.sampleType} onIonChange={e => setWellForm(f => ({ ...f, sampleType: e.detail.value }))}>
              <IonSelectOption value="sample">Sample</IonSelectOption>
              <IonSelectOption value="control">Control</IonSelectOption>
              <IonSelectOption value="NTC">NTC (No Template Control)</IonSelectOption>
              <IonSelectOption value="standard">Standard</IonSelectOption>
            </IonSelect>
          </IonItem>
          <IonItem>
            <IonLabel position="stacked">Replicate Group</IonLabel>
            <IonInput value={wellForm.replicateGroup} onIonInput={e => setWellForm(f => ({ ...f, replicateGroup: e.detail.value ?? '' }))} placeholder="e.g. 1, 2, 3" clearInput />
          </IonItem>
          <IonButton expand="block" style={{ marginTop: 20 }} onClick={saveWell} disabled={updateWell.isPending}>
            {updateWell.isPending ? <IonSpinner name="crescent" /> : 'Save Well'}
          </IonButton>
        </IonContent>
      </IonModal>

      {/* Quick Fill Modal */}
      <IonModal isOpen={showQuickFill} onDidDismiss={() => setShowQuickFill(false)} breakpoints={[0, 0.85]} initialBreakpoint={0.85}>
        <IonHeader>
          <IonToolbar>
            <IonTitle>Quick Fill</IonTitle>
            <IonButtons slot="end">
              <IonButton onClick={() => setShowQuickFill(false)}>Cancel</IonButton>
            </IonButtons>
          </IonToolbar>
        </IonHeader>
        <IonContent className="ion-padding">
          <IonText color="medium"><p style={{ fontSize: 12, marginTop: 0 }}>Fill a range of wells with the same data.</p></IonText>
          <div style={{ display: 'flex', gap: 8 }}>
            <IonItem style={{ flex: 1 }}>
              <IonLabel position="stacked">From Well</IonLabel>
              <IonInput value={qfForm.fromWell} onIonInput={e => setQfForm(f => ({ ...f, fromWell: e.detail.value ?? '' }))} placeholder="A01" />
            </IonItem>
            <IonItem style={{ flex: 1 }}>
              <IonLabel position="stacked">To Well</IonLabel>
              <IonInput value={qfForm.toWell} onIonInput={e => setQfForm(f => ({ ...f, toWell: e.detail.value ?? '' }))} placeholder="A12" />
            </IonItem>
          </div>
          <IonItem>
            <IonLabel position="stacked">Sample Name</IonLabel>
            <IonInput value={qfForm.sampleName} onIonInput={e => setQfForm(f => ({ ...f, sampleName: e.detail.value ?? '' }))} placeholder="e.g. Patient_Group_A" clearInput />
          </IonItem>
          <IonItem>
            <IonLabel position="stacked">Target Gene</IonLabel>
            <IonInput value={qfForm.targetGene} onIonInput={e => setQfForm(f => ({ ...f, targetGene: e.detail.value ?? '' }))} placeholder="e.g. CCR2" clearInput />
          </IonItem>
          <IonItem>
            <IonLabel position="stacked">Sample Type</IonLabel>
            <IonSelect value={qfForm.sampleType} onIonChange={e => setQfForm(f => ({ ...f, sampleType: e.detail.value }))}>
              <IonSelectOption value="sample">Sample</IonSelectOption>
              <IonSelectOption value="control">Control</IonSelectOption>
              <IonSelectOption value="NTC">NTC</IonSelectOption>
              <IonSelectOption value="standard">Standard</IonSelectOption>
            </IonSelect>
          </IonItem>
          <IonItem>
            <IonLabel position="stacked">Replicate Group</IonLabel>
            <IonInput value={qfForm.replicateGroup} onIonInput={e => setQfForm(f => ({ ...f, replicateGroup: e.detail.value ?? '' }))} placeholder="1" clearInput />
          </IonItem>
          <IonButton expand="block" style={{ marginTop: 20 }} onClick={handleQuickFill} disabled={quickFill.isPending}>
            {quickFill.isPending ? <IonSpinner name="crescent" /> : 'Fill Wells'}
          </IonButton>
        </IonContent>
      </IonModal>
    </>
  );
};

export default PlateSetupTab;
