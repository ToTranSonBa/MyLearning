import { Component, inject } from '@angular/core';
import { AuthService } from '../../../../core/services/auth.service';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { CardComponent } from '../../../../shared/components/card/card.component';

@Component({
  selector: 'app-home',
  imports: [InputComponent, LoadingComponent, CardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  auth = inject(AuthService);
}
