import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { InviteInfo } from '../../../core/models/invite.model';
import { InviteService } from '../../../core/services/invite.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  inviteInfo: InviteInfo | null = null;
  tokenError: string | null = null;
  token = '';
  loading = false;
  success = false;
  showPassword = false;
  showConfirm = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private inviteService: InviteService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!this.token) {
      this.tokenError = 'No invite token provided.';
      return;
    }

    this.inviteService.validateToken(this.token).subscribe({
      next: info => {
        this.inviteInfo = info;
        this.buildForm(info.email);
      },
      error: err => {
        this.tokenError = err?.error?.title ?? 'This invite link is invalid or has expired.';
      }
    });
  }

  private buildForm(email: string): void {
    this.form = this.fb.group({
      email: [{ value: email, disabled: true }],
      name: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, this.passwordStrengthValidator]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordsMatchValidator });
  }

  private passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const val: string = control.value ?? '';
    if (val.length < 8) return { minLength: true };
    if (!/[A-Z]/.test(val)) return { noUppercase: true };
    if (!/[0-9]/.test(val)) return { noDigit: true };
    if (!/[^a-zA-Z0-9]/.test(val)) return { noSpecial: true };
    return null;
  }

  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const pw = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return pw && confirm && pw !== confirm ? { passwordMismatch: true } : null;
  }

  get passwordStrength(): 'weak' | 'fair' | 'strong' {
    const val: string = this.form?.get('password')?.value ?? '';
    let score = 0;
    if (val.length >= 8) score++;
    if (/[A-Z]/.test(val)) score++;
    if (/[0-9]/.test(val)) score++;
    if (/[^a-zA-Z0-9]/.test(val)) score++;
    if (score <= 1) return 'weak';
    if (score <= 3) return 'fair';
    return 'strong';
  }

  submit(): void {
    if (!this.form || this.form.invalid) return;

    this.loading = true;
    this.inviteService.acceptInvite({
      token: this.token,
      name: this.form.get('name')!.value,
      password: this.form.get('password')!.value
    }).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
      },
      error: err => {
        this.loading = false;
        const errors = err?.error?.errors;
        if (errors) {
          const messages = Object.values(errors).flat().join(' ');
          this.form.setErrors({ serverError: messages });
        } else {
          this.form.setErrors({ serverError: err?.error?.title ?? 'Registration failed.' });
        }
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
