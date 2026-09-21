import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BranchDto } from '../../../apis/branches/branch.dto';

@Component({
  selector: 'app-branch-select',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './branch-select.html'
})
export class BranchSelect {
  branches = input.required<BranchDto[]>();
  selectedBranchId = input<number | null>(null);
  branchChanged = output<number | null>();

  onChange(branchId: number | null): void {
    this.branchChanged.emit(branchId);
  }
}
