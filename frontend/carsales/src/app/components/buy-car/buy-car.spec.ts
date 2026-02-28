import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BuyCar } from './buy-car';

describe('BuyCar', () => {
  let component: BuyCar;
  let fixture: ComponentFixture<BuyCar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BuyCar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BuyCar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
