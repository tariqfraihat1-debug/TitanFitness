import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { BranchDto } from '../../../apis/branches/branch.dto';
import { BranchesService } from '../../../apis/branches/branches.service';
import { CreateMemberDto } from '../../../apis/members/create-member.dto';
import { MembersService } from '../../../apis/members/members.service';
import { UpdateMemberDto } from '../../../apis/members/update-member.dto';
import { AlertMessage } from '../../../shared/components/alert-message/alert-message';
import { BackNavigation } from '../../../shared/components/back-navigation/back-navigation';
import { Button } from '../../../shared/components/button/button';
import { Editor } from '../../../shared/components/editor/editor';
import { InfoNotice } from '../../../shared/components/info-notice/info-notice';
import { Loading } from '../../../shared/components/loading/loading';
import { MemberPhoto } from '../../../shared/components/member-photo/member-photo';
import { ModeBadge } from '../../../shared/components/mode-badge/mode-badge';
import { SelectEditor } from '../../../shared/components/select-editor/select-editor';

@Component({
  selector: 'app-member-details',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AlertMessage,
    BackNavigation,
    Button,
    Editor,
    InfoNotice,
    Loading,
    MemberPhoto,
    ModeBadge,
    SelectEditor
  ],
  templateUrl: './member-details.html'
})
export class MemberDetails implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly branchesService = inject(BranchesService);
  private readonly membersService = inject(MembersService);

  mode = signal<'create' | 'edit'>('create');
  memberId = signal<number | null>(null);
  branches = signal<BranchDto[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal('');

  readonly isEdit = computed(() =>
    this.mode() === 'edit'
  );

  readonly branchOptions = computed(() =>
    this.branches().map(branch => ({
      value: branch.branchId,
      label: branch.name
    }))
  );

  readonly form = this.fb.group({
    fullName: ['', [
      Validators.required,
      Validators.maxLength(100)
    ]],
    membershipNumber: ['', [
      Validators.maxLength(10)
    ]],
    email: ['', [
      Validators.email,
      Validators.maxLength(100)
    ]],
    phone: ['', [
      Validators.maxLength(20)
    ]],
    address: ['', [
      Validators.maxLength(200)
    ]],
    joinedDate: ['', Validators.required],
    homeBranchId: [
      null as number | null,
      Validators.required
    ],
    photo: ['']
  });

  readonly fields = computed(() => [
    {
      label: 'Full Name',
      control: this.form.controls.fullName,
      type: 'text' as const,
      placeholder: 'e.g., Jane Doe',
      helper: '',
      requiredMark: true,
      readonly: false,
      errorMessage: 'Full name is required.',
      col: 'col-12 col-md-6'
    },
    {
      label: 'Membership Number',
      control: this.form.controls.membershipNumber,
      type: 'text' as const,
      placeholder: 'TF-____',
      helper: this.isEdit()
        ? 'Unique and never changes once the member is created.'
        : 'Unique, max 10 characters (e.g. TF-8932). Generated automatically if left blank.',
      requiredMark: false,
      readonly: this.isEdit(),
      errorMessage: '',
      col: 'col-12 col-md-6'
    },
    {
      label: 'Email',
      control: this.form.controls.email,
      type: 'email' as const,
      placeholder: 'name@email.com',
      helper: '',
      requiredMark: false,
      readonly: false,
      errorMessage: 'Enter a valid email address.',
      col: 'col-12 col-md-6'
    },
    {
      label: 'Phone',
      control: this.form.controls.phone,
      type: 'tel' as const,
      placeholder: '+1 (555) 000-0000',
      helper: '',
      requiredMark: false,
      readonly: false,
      errorMessage: '',
      col: 'col-12 col-md-6'
    },
    {
      label: 'Address',
      control: this.form.controls.address,
      type: 'text' as const,
      placeholder: 'Street, city, apt.',
      helper: '',
      requiredMark: false,
      readonly: false,
      errorMessage: '',
      col: 'col-12'
    },
    {
      label: 'Joined Date',
      control: this.form.controls.joinedDate,
      type: 'date' as const,
      placeholder: '',
      helper: '',
      requiredMark: true,
      readonly: false,
      errorMessage: 'Joined date is required.',
      col: 'col-12 col-md-6'
    }
  ]);

  // Reads the mode and member ID, then loads the page data.
  ngOnInit(): void {
    const mode = this.route.snapshot.queryParamMap.get('mode');
    const memberId = this.route.snapshot.queryParamMap.get('memberId');

    this.mode.set(
      mode === 'edit'
        ? 'edit'
        : 'create'
    );

    if (this.isEdit()) {
      const id = Number(memberId);

      if (!Number.isInteger(id) || id <= 0) {
        this.router.navigate(['/members']);
        return;
      }

      this.memberId.set(id);
    }

    this.loadBranches();

    if (this.isEdit())
      this.loadMember(this.memberId()!);
  }

  // Opens the file explorer and stores the selected image.
  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file)
      return;

    if (!file.type.startsWith('image/')) {
      this.error.set('Please select a valid image file.');
      input.value = '';
      return;
    }

    const reader = new FileReader();

    reader.onload = () => {
      this.form.controls.photo.setValue(reader.result as string);
      this.form.controls.photo.markAsDirty();
      this.error.set('');
      input.value = '';
    };

    reader.onerror = () => {
      this.error.set('Unable to read the selected photo.');
      input.value = '';
    };

    reader.readAsDataURL(file);
  }

  // Returns to the profile or members list.
  back(): void {
    const memberId = this.memberId();

    if (this.isEdit() && memberId) {
      this.router.navigate(['/members/profile'], {
        queryParams: { memberId }
      });
      return;
    }

    this.router.navigate(['/members']);
  }

  // Cancels the form and returns to the previous screen.
  cancel(): void {
    this.back();
  }

  // Validates the form and creates or updates the member.
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.isEdit()) {
      this.updateMember();
      return;
    }

    this.createMember();
  }

  // Creates a new member from the form values.
  private createMember(): void {
    const value = this.form.getRawValue();

    const member: CreateMemberDto = {
      membershipNumber: value.membershipNumber?.trim() || null,
      fullName: value.fullName!.trim(),
      email: value.email?.trim() || null,
      phone: value.phone?.trim() || null,
      address: value.address?.trim() || null,
      joinedDate: value.joinedDate!,
      photo: value.photo?.trim() || null,
      homeBranchId: value.homeBranchId!
    };

    this.saving.set(true);
    this.error.set('');

    this.membersService.createMember(member)
      .pipe(
        finalize(() =>
          this.saving.set(false)
        )
      )
      .subscribe({
        next: response =>
          this.router.navigate(['/members/profile'], {
            queryParams: {
              memberId: response.data
            }
          }),
        error: error =>
          this.error.set(error.message)
      });
  }

  // Updates the selected member with the form values.
  private updateMember(): void {
    const memberId = this.memberId();

    if (!memberId)
      return;

    const value = this.form.getRawValue();

    const member: UpdateMemberDto = {
      fullName: value.fullName!.trim(),
      email: value.email?.trim() || null,
      phone: value.phone?.trim() || null,
      address: value.address?.trim() || null,
      joinedDate: value.joinedDate!,
      photo: value.photo?.trim() || null,
      homeBranchId: value.homeBranchId!
    };

    this.saving.set(true);
    this.error.set('');

    this.membersService.updateMember(
      memberId,
      member
    )
      .pipe(
        finalize(() =>
          this.saving.set(false)
        )
      )
      .subscribe({
        next: () =>
          this.router.navigate(['/members/profile'], {
            queryParams: { memberId }
          }),
        error: error =>
          this.error.set(error.message)
      });
  }

  // Loads the selected member into the form.
  private loadMember(memberId: number): void {
    this.loading.set(true);
    this.error.set('');

    this.membersService.getMemberById(memberId)
      .pipe(
        finalize(() =>
          this.loading.set(false)
        )
      )
      .subscribe({
        next: response => {
          const member = response.data;

          this.form.patchValue({
            fullName: member.fullName,
            membershipNumber: member.membershipNumber,
            email: member.email,
            phone: member.phone,
            address: member.address,
            joinedDate: member.joinedDate,
            homeBranchId: member.homeBranchId,
            photo: member.photo
          });
        },
        error: error =>
          this.error.set(error.message)
      });
  }

  // Loads branches for the home branch selector.
  private loadBranches(): void {
    this.branchesService.getBranches()
      .subscribe({
        next: response =>
          this.branches.set(response.data),
        error: error =>
          this.error.set(error.message)
      });
  }
}
